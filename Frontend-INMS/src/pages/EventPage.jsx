import React, { useEffect, useState } from "react";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";
import AlarmService from "../services/AlarmService";
import DeviceService from "../services/DeviceService";
import EventLogService from "../services/EventLogService";
import { formatDate, getDeviceTypeLabel } from "../utils/formatters";

const normalizeEventValue = (value, fallback = "UNKNOWN") => {
    const normalized = String(value ?? "").trim().toUpperCase();
    return normalized || fallback;
};

const formatAlarmLabel = (alarmType) => {
    const normalized = normalizeEventValue(alarmType, "");
    return normalized ? normalized.replaceAll("_", " ") : "ALARM";
};

const getEventSeverity = (eventType, alarmType) => {
    const normalizedEventType = normalizeEventValue(eventType);
    const normalizedAlarmType = normalizeEventValue(alarmType, "");

    if (normalizedAlarmType === "NODE_DOWN") return "CRITICAL";
    if (normalizedEventType === "HEARTBEAT_TIMEOUT" || normalizedEventType === "NODE_DOWN") return "CRITICAL";

    if (
        normalizedAlarmType === "NODE_UNREACHABLE" ||
        normalizedEventType === "SIMULATED_DOWN" ||
        normalizedEventType === "ALARM_RAISED"
    ) {
        return "MAJOR";
    }

    if (
        normalizedEventType === "HEARTBEAT_RECOVERED" ||
        normalizedEventType === "ALARM_CLEARED" ||
        normalizedEventType === "NODE_UP"
    ) {
        return "INFO";
    }

    return "NOTICE";
};

const getEventMessage = (eventType, alarmType, nodeName) => {
    const normalizedEventType = normalizeEventValue(eventType);
    const alarmLabel = formatAlarmLabel(alarmType);

    switch (normalizedEventType) {
        case "ALARM_RAISED":
            return `${alarmLabel} raised for ${nodeName}`;
        case "ALARM_CLEARED":
            return `${alarmLabel} cleared for ${nodeName}`;
        case "HEARTBEAT_TIMEOUT":
            return `Heartbeat timeout detected for ${nodeName}`;
        case "HEARTBEAT_RECOVERED":
            return `Heartbeat recovered for ${nodeName}`;
        case "SIMULATED_DOWN":
            return `Simulation forced ${nodeName} into a down state`;
        case "NODE_DOWN":
            return `${nodeName} is down`;
        case "NODE_UP":
            return `${nodeName} is back online`;
        default:
            return `${normalizedEventType.replaceAll("_", " ")} recorded for ${nodeName}`;
    }
};

const EventPage = () => {
    const [search, setSearch] = useState("");
    const [selectedNode, setSelectedNode] = useState("All");
    const [selectedDate, setSelectedDate] = useState(null);
    const [events, setEvents] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [lastRefreshed, setLastRefreshed] = useState(null);

    useEffect(() => {
        loadEventLogs();
    }, []);

    const loadEventLogs = async () => {
        setLoading(true);
        setError(null);

        try {
            const [eventRows, deviceRows, alarmRows] = await Promise.all([
                EventLogService.getAll(),
                DeviceService.getAll(),
                AlarmService.getAll()
            ]);

            const deviceMap = new Map(deviceRows.map((device) => [device.deviceId, device]));
            const alarmMap = new Map(alarmRows.map((alarm) => [alarm.alarmId, alarm]));

            const mappedEvents = eventRows
                .map((eventRow) => {
                    const device = deviceMap.get(eventRow.deviceId);
                    const alarm = eventRow.alarmId ? alarmMap.get(eventRow.alarmId) : null;
                    const nodeName = device?.deviceName ?? `Device #${eventRow.deviceId}`;

                    return {
                        id: eventRow.eventId,
                        timestamp: eventRow.eventTime,
                        timeLabel: formatDate(eventRow.eventTime),
                        node: nodeName,
                        type: getDeviceTypeLabel(device?.deviceType ?? "Unknown"),
                        event: normalizeEventValue(eventRow.eventType),
                        officer: device?.assignedUserFullName ?? "Unassigned",
                        severity: getEventSeverity(eventRow.eventType, alarm?.alarmType),
                        msg: getEventMessage(eventRow.eventType, alarm?.alarmType, nodeName)
                    };
                })
                .sort((left, right) => new Date(right.timestamp).getTime() - new Date(left.timestamp).getTime());

            setEvents(mappedEvents);
            setLastRefreshed(new Date());
        } catch (err) {
            console.error(err);
            setError("Failed to load event logs. Please make sure the backend API is running.");
            setEvents([]);
        } finally {
            setLoading(false);
        }
    };

    const nodes = ["All", ...new Set(events.map((event) => event.node))];

    const filtered = events.filter((event) => {
        const eventDate = new Date(event.timestamp);
        const searchTerm = search.trim().toLowerCase();

        return (
            (selectedNode === "All" || event.node === selectedNode) &&
            (
                !searchTerm ||
                event.node.toLowerCase().includes(searchTerm) ||
                event.msg.toLowerCase().includes(searchTerm) ||
                event.event.toLowerCase().includes(searchTerm) ||
                event.officer.toLowerCase().includes(searchTerm)
            ) &&
            (!selectedDate || eventDate.toDateString() === selectedDate.toDateString())
        );
    });

    const getSeverityRowClass = (severity) => {
        if (severity === "CRITICAL") return "hover:bg-red-50";
        if (severity === "MAJOR") return "hover:bg-yellow-50";
        if (severity === "INFO") return "hover:bg-blue-50";
        return "hover:bg-gray-50";
    };

    const getSeverityBadge = (severity) => {
        if (severity === "CRITICAL") return "bg-red-600 text-white";
        if (severity === "MAJOR") return "bg-yellow-600 text-white";
        if (severity === "INFO") return "bg-blue-600 text-white";
        return "bg-gray-500 text-white";
    };

    return (
        <div className="min-h-screen bg-gray-50 p-6 space-y-6">
            <div>
                <h1 className="text-2xl font-semibold text-gray-800">
                    System Event Log
                </h1>
                <p className="mt-0.5 text-sm text-gray-500">
                    Monitor all system events and alarm activities across your network
                </p>
            </div>

            {error && (
                <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
                    {error}
                </div>
            )}

            <div className="rounded-lg border border-gray-200 bg-white p-4 shadow-sm">
                <div className="flex flex-col gap-3 md:flex-row md:items-center">
                    <select
                        value={selectedNode}
                        onChange={(e) => setSelectedNode(e.target.value)}
                        className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        {nodes.map((nodeName) => (
                            <option key={nodeName} value={nodeName}>{nodeName}</option>
                        ))}
                    </select>

                    <div className="flex flex-1 items-center overflow-hidden rounded-lg border border-gray-300 bg-white">
                        <input
                            type="text"
                            placeholder="Search node, message, event, or officer..."
                            value={search}
                            onChange={(e) => setSearch(e.target.value)}
                            className="flex-1 px-3 py-2 text-sm focus:outline-none"
                        />

                        <button
                            type="button"
                            onClick={loadEventLogs}
                            className="bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700"
                        >
                            Refresh
                        </button>
                    </div>

                    <div className="flex items-center gap-2">
                        <DatePicker
                            selected={selectedDate}
                            onChange={(date) => setSelectedDate(date)}
                            placeholderText="Filter by date"
                            dateFormat="dd/MM/yyyy"
                            className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                            popperPlacement="bottom-start"
                        />

                        <button
                            type="button"
                            onClick={() => setSelectedDate(null)}
                            className="text-xs text-blue-600 hover:underline"
                        >
                            Select All
                        </button>
                    </div>
                </div>
            </div>

            <div className="overflow-hidden rounded-lg border border-gray-200 bg-white shadow-sm">
                <div className="border-b border-gray-200 p-4">
                    <div className="flex flex-col gap-1 md:flex-row md:items-center md:justify-between">
                        <h2 className="text-base font-semibold text-gray-700">
                            Events ({filtered.length})
                        </h2>
                        {lastRefreshed && (
                            <p className="text-xs text-gray-400">
                                Last refreshed: {formatDate(lastRefreshed.toISOString())}
                            </p>
                        )}
                    </div>
                </div>

                <div className="overflow-x-auto">
                    <table className="w-full text-left text-sm">
                        <thead>
                            <tr className="border-b border-gray-200 bg-gray-50">
                                <th className="px-4 py-3 text-xs font-semibold uppercase text-gray-600">Timestamp</th>
                                <th className="px-4 py-3 text-xs font-semibold uppercase text-gray-600">Node</th>
                                <th className="px-4 py-3 text-xs font-semibold uppercase text-gray-600">Type</th>
                                <th className="px-4 py-3 text-xs font-semibold uppercase text-gray-600">Event Type</th>
                                <th className="px-4 py-3 text-xs font-semibold uppercase text-gray-600">Officer</th>
                                <th className="px-4 py-3 text-xs font-semibold uppercase text-gray-600">Severity</th>
                                <th className="px-4 py-3 text-xs font-semibold uppercase text-gray-600">Message</th>
                            </tr>
                        </thead>

                        <tbody>
                            {loading ? (
                                <tr>
                                    <td colSpan={7} className="px-4 py-8 text-center text-sm text-gray-400">
                                        Loading event logs...
                                    </td>
                                </tr>
                            ) : filtered.length === 0 ? (
                                <tr>
                                    <td colSpan={7} className="px-4 py-8 text-center text-sm text-gray-400">
                                        No events found.
                                    </td>
                                </tr>
                            ) : (
                                filtered.map((event) => (
                                    <tr
                                        key={event.id}
                                        className={`border-b border-gray-100 transition ${getSeverityRowClass(event.severity)}`}
                                    >
                                        <td className="px-4 py-3 font-mono text-xs text-gray-600">{event.timeLabel}</td>
                                        <td className="px-4 py-3 font-medium text-gray-800">{event.node}</td>
                                        <td className="px-4 py-3">
                                            <span className="rounded bg-purple-100 px-2 py-1 text-xs font-semibold text-purple-700">
                                                {event.type}
                                            </span>
                                        </td>
                                        <td className="px-4 py-3">
                                            <span className="rounded bg-gray-100 px-2 py-1 text-xs font-semibold text-gray-700">
                                                {event.event}
                                            </span>
                                        </td>
                                        <td className="px-4 py-3 text-gray-600">{event.officer}</td>
                                        <td className="px-4 py-3">
                                            <span className={`rounded px-3 py-1 text-xs font-semibold ${getSeverityBadge(event.severity)}`}>
                                                {event.severity}
                                            </span>
                                        </td>
                                        <td className="max-w-md px-4 py-3 text-gray-600">{event.msg}</td>
                                    </tr>
                                ))
                            )}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
};

export default EventPage;
