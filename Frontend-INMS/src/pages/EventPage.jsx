import React, { useState } from "react";
import DatePicker from "react-datepicker";
import "react-datepicker/dist/react-datepicker.css";

const EventPage = () => {
    const [search, setSearch] = useState("");
    const [selectedNode, setSelectedNode] = useState("All");
    const [selectedDate, setSelectedDate] = useState(null);

    const data = [
        { time: "13/02/2026, 10:23:03", node: "CEAN-BLR-001", type: "CEAN", event: "ALARM_RAISED", officer: "John Silva", severity: "MAJOR", msg: "AC alarm raised" },
        { time: "13/02/2026, 10:23:02", node: "CEAN-BLR-001", type: "CEAN", event: "ALARM_RAISED", officer: "John Silva", severity: "MAJOR", msg: "AC power failure" },
        { time: "13/02/2026, 10:22:56", node: "CEAN-BLR-001", type: "CEAN", event: "NODE_DOWN", officer: "Mary Fernando", severity: "CRITICAL", msg: "Node DOWN" },
        { time: "13/02/2026, 10:20:10", node: "SLBN-DEL-001", type: "SLBN", event: "NODE_DOWN", officer: "Kamal Perera", severity: "CRITICAL", msg: "Node DOWN" },
        { time: "13/02/2026, 10:18:20", node: "SLBN-DEL-001", type: "SLBN", event: "NODE_UP", officer: "Kamal Perera", severity: "INFO", msg: "Node UP" },
        { time: "13/02/2026, 10:15:05", node: "MSAN-BLR-001", type: "MSAN", event: "ALARM_RAISED", officer: "Nimal Silva", severity: "MAJOR", msg: "Temperature alert" },
        { time: "13/02/2026, 10:14:00", node: "MSAN-BLR-001", type: "MSAN", event: "NODE_UP", officer: "Nimal Silva", severity: "INFO", msg: "Node UP" }
    ];

    const nodes = ["All", ...new Set(data.map((d) => d.node))];

    const filtered = data.filter((d) => {
        const eventDate = new Date(d.time);

        return (
            (selectedNode === "All" || d.node === selectedNode) &&
            (d.node.toLowerCase().includes(search.toLowerCase()) ||
             d.msg.toLowerCase().includes(search.toLowerCase())) &&
            (!selectedDate ||
             eventDate.toDateString() === selectedDate.toDateString())
        );
    });

    const getSeverityRowClass = (sev) => {
        if (sev === "CRITICAL") return "hover:bg-red-50";
        if (sev === "MAJOR") return "hover:bg-yellow-50";
        if (sev === "INFO") return "hover:bg-blue-50";
        return "hover:bg-gray-50";
    };

    const getSeverityBadge = (sev) => {
        if (sev === "CRITICAL") return "bg-red-600 text-white";
        if (sev === "MAJOR") return "bg-yellow-600 text-white";
        if (sev === "INFO") return "bg-blue-600 text-white";
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

            <div className="rounded-lg border border-gray-200 bg-white p-4 shadow-sm">
                <div className="flex flex-col gap-3 md:flex-row md:items-center">
                    <select
                        value={selectedNode}
                        onChange={(e) => setSelectedNode(e.target.value)}
                        className="rounded-lg border border-gray-300 px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        {nodes.map((n, i) => (
                            <option key={i} value={n}>{n}</option>
                        ))}
                    </select>

                    <div className="flex flex-1 items-center overflow-hidden rounded-lg border border-gray-300">
                        <input
                            type="text"
                            placeholder="Search node or message..."
                            value={search}
                            onChange={(e) => setSearch(e.target.value)}
                            className="flex-1 px-3 py-2 text-sm focus:outline-none"
                        />

                        <button className="bg-blue-600 px-4 py-2 text-sm text-white hover:bg-blue-700">
                            Search
                        </button>
                    </div>

                    <div className="relative flex items-center gap-2">
                        <DatePicker
                            selected={selectedDate}
                            onChange={(date) => setSelectedDate(date)}
                            placeholderText="📅"
                            className="h-10 w-10 cursor-pointer rounded-lg border border-gray-300 text-center hover:bg-gray-50"
                            popperPlacement="bottom-start"
                        />

                        <button
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
                    <h2 className="text-base font-semibold text-gray-700">
                        Events ({filtered.length}) 
                        
                    </h2>
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
                            {filtered.length === 0 ? (
                                <tr>
                                    <td colSpan={7} className="px-4 py-8 text-center text-sm text-gray-400">
                                        No events found.
                                    </td>
                                </tr>
                            ) : (
                                filtered.map((d, i) => (
                                    <tr
                                        key={i}
                                        className={`border-b border-gray-100 transition ${getSeverityRowClass(d.severity)}`}
                                    >
                                        <td className="px-4 py-3 font-mono text-xs text-gray-600">{d.time}</td>
                                        <td className="px-4 py-3 font-medium text-gray-800">{d.node}</td>
                                        <td className="px-4 py-3">
                                            <span className="rounded bg-purple-100 px-2 py-1 text-xs font-semibold text-purple-700">
                                                {d.type}
                                            </span>
                                        </td>
                                        <td className="px-4 py-3">
                                            <span className="rounded bg-gray-100 px-2 py-1 text-xs font-semibold text-gray-700">
                                                {d.event}
                                            </span>
                                        </td>
                                        <td className="px-4 py-3 text-gray-600">{d.officer}</td>
                                        <td className="px-4 py-3">
                                            <span className={`rounded px-3 py-1 text-xs font-semibold ${getSeverityBadge(d.severity)}`}>
                                                {d.severity}
                                            </span>
                                        </td>
                                        <td className="max-w-xs truncate px-4 py-3 text-gray-600">{d.msg}</td>
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
