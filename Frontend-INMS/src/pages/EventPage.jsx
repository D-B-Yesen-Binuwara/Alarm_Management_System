import React, { useState } from "react";

const EventPage = () => {
    const [search, setSearch] = useState("");
    const [severity, setSeverity] = useState("All");
    const [type, setType] = useState("All");
    const [eventType, setEventType] = useState("All");

    // 🔥 FULL DATA (CEAN + SLBN + MSAN)
    const data = [
        { time: "13/02/2026, 10:23:03", node: "CEAN-BLR-001", type: "CEAN", event: "ALARM_RAISED", officer: "John Silva", severity: "MAJOR", msg: "AC alarm raised" },
        { time: "13/02/2026, 10:23:02", node: "CEAN-BLR-001", type: "CEAN", event: "ALARM_RAISED", officer: "John Silva", severity: "MAJOR", msg: "AC power failure" },
        { time: "13/02/2026, 10:22:56", node: "CEAN-BLR-001", type: "CEAN", event: "NODE_DOWN", officer: "Mary Fernando", severity: "CRITICAL", msg: "Node DOWN" },

        { time: "13/02/2026, 10:20:10", node: "SLBN-DEL-001", type: "SLBN", event: "NODE_DOWN", officer: "Kamal Perera", severity: "CRITICAL", msg: "Node DOWN" },
        { time: "13/02/2026, 10:18:20", node: "SLBN-DEL-001", type: "SLBN", event: "NODE_UP", officer: "Kamal Perera", severity: "INFO", msg: "Node UP" },

        { time: "13/02/2026, 10:15:05", node: "MSAN-BLR-001", type: "MSAN", event: "ALARM_RAISED", officer: "Nimal Silva", severity: "MAJOR", msg: "Temperature alert" },
        { time: "13/02/2026, 10:14:00", node: "MSAN-BLR-001", type: "MSAN", event: "NODE_UP", officer: "Nimal Silva", severity: "INFO", msg: "Node UP" }
    ];

    // 🔍 UNIQUE TYPES
    const types = ["All", ...new Set(data.map((d) => d.type))];
    const events = ["All", ...new Set(data.map((d) => d.event))];

    // 🔍 FILTER
    const filtered = data.filter((d) => {
        return (
            (d.node.toLowerCase().includes(search.toLowerCase()) ||
                d.msg.toLowerCase().includes(search.toLowerCase())) &&
            (severity === "All" || d.severity === severity) &&
            (type === "All" || d.type === type) &&
            (eventType === "All" || d.event === eventType)
        );
    });

    // 🎨 GET ROW COLOR CLASS
    const getSeverityRowClass = (sev) => {
        if (sev === "CRITICAL") return "hover:bg-red-50";
        if (sev === "MAJOR") return "hover:bg-yellow-50";
        if (sev === "INFO") return "hover:bg-blue-50";
        return "hover:bg-gray-50";
    };

    // 🎨 GET SEVERITY BADGE CLASS
    const getSeverityBadge = (sev) => {
        if (sev === "CRITICAL") return "bg-red-600 text-white";
        if (sev === "MAJOR") return "bg-yellow-600 text-white";
        if (sev === "INFO") return "bg-blue-600 text-white";
        return "bg-gray-500 text-white";
    };

    // Stats
    const stats = {
        total: data.length,
        critical: data.filter(d => d.severity === "CRITICAL").length,
        major: data.filter(d => d.severity === "MAJOR").length,
        info: data.filter(d => d.severity === "INFO").length,
    };

    return (
        <div className="min-h-screen bg-gray-50 p-6 space-y-6">
            {/* Title */}
            <div>
                <h1 className="text-2xl font-semibold text-gray-800">
                    System Event Log
                </h1>
                <p className="text-gray-500 text-sm mt-0.5">
                    Monitor all system events and alarm activities across your network
                </p>
            </div>

            {/* Summary Cards */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
                <div className="border-sky-200 bg-gradient-to-br from-sky-50 to-cyan-50 p-4 rounded-lg border shadow-sm">
                    <div className="text-sm text-sky-700 font-medium">Total Events</div>
                    <div className="text-2xl font-bold text-sky-900 mt-2">{stats.total}</div>
                </div>
                <div className="border-rose-200 bg-gradient-to-br from-rose-50 to-orange-50 p-4 rounded-lg border shadow-sm">
                    <div className="text-sm text-rose-700 font-medium">Critical</div>
                    <div className="text-2xl font-bold text-rose-900 mt-2">{stats.critical}</div>
                </div>
                <div className="border-amber-200 bg-gradient-to-br from-amber-50 to-yellow-50 p-4 rounded-lg border shadow-sm">
                    <div className="text-sm text-amber-700 font-medium">Major</div>
                    <div className="text-2xl font-bold text-amber-900 mt-2">{stats.major}</div>
                </div>
                <div className="border-emerald-200 bg-gradient-to-br from-emerald-50 to-lime-50 p-4 rounded-lg border shadow-sm">
                    <div className="text-sm text-emerald-700 font-medium">Info</div>
                    <div className="text-2xl font-bold text-emerald-900 mt-2">{stats.info}</div>
                </div>
            </div>

            {/* Filters */}
            <div className="bg-white p-4 rounded-lg border border-gray-200 shadow-sm">
                <div className="flex flex-col md:flex-row gap-3">
                    {/* Type Filter */}
                    <select
                        value={type}
                        onChange={(e) => setType(e.target.value)}
                        className="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        {types.map((t, i) => (
                            <option key={i}>{t}</option>
                        ))}
                    </select>

                    {/* Event Type Filter */}
                    <select
                        value={eventType}
                        onChange={(e) => setEventType(e.target.value)}
                        className="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        {events.map((e, i) => (
                            <option key={i}>{e}</option>
                        ))}
                    </select>

                    {/* Severity Filter */}
                    <select
                        value={severity}
                        onChange={(e) => setSeverity(e.target.value)}
                        className="px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    >
                        <option>All</option>
                        <option>CRITICAL</option>
                        <option>MAJOR</option>
                        <option>INFO</option>
                    </select>

                    {/* Search */}
                    <input
                        type="text"
                        placeholder="Search node or message..."
                        value={search}
                        onChange={(e) => setSearch(e.target.value)}
                        className="flex-1 px-3 py-2 text-sm border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                    />

                    <button className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded-lg transition whitespace-nowrap">
                        Refresh
                    </button>
                </div>
            </div>

            {/* Events Table */}
            <div className="bg-white rounded-lg border border-gray-200 shadow-sm overflow-hidden">
                <div className="p-4 border-b border-gray-200">
                    <h2 className="text-base font-semibold text-gray-700">
                        Events ({filtered.length})
                    </h2>
                </div>

                <div className="overflow-x-auto">
                    <table className="w-full text-sm text-left">
                        <thead>
                            <tr className="border-b border-gray-200 bg-gray-50">
                                <th className="px-4 py-3 font-semibold text-gray-600 text-xs uppercase">Timestamp</th>
                                <th className="px-4 py-3 font-semibold text-gray-600 text-xs uppercase">Node</th>
                                <th className="px-4 py-3 font-semibold text-gray-600 text-xs uppercase">Type</th>
                                <th className="px-4 py-3 font-semibold text-gray-600 text-xs uppercase">Event Type</th>
                                <th className="px-4 py-3 font-semibold text-gray-600 text-xs uppercase">Officer</th>
                                <th className="px-4 py-3 font-semibold text-gray-600 text-xs uppercase">Severity</th>
                                <th className="px-4 py-3 font-semibold text-gray-600 text-xs uppercase">Message</th>
                            </tr>
                        </thead>

                        <tbody>
                            {filtered.length === 0 ? (
                                <tr>
                                    <td colSpan={7} className="px-4 py-8 text-center text-gray-400 text-sm">
                                        No events found.
                                    </td>
                                </tr>
                            ) : (
                                filtered.map((d, i) => (
                                    <tr
                                        key={i}
                                        className={`border-b border-gray-100 transition ${getSeverityRowClass(d.severity)}`}
                                    >
                                        <td className="px-4 py-3 text-xs text-gray-600 font-mono">{d.time}</td>
                                        <td className="px-4 py-3 font-medium text-gray-800">{d.node}</td>
                                        <td className="px-4 py-3">
                                            <span className="bg-purple-100 text-purple-700 text-xs font-semibold px-2 py-1 rounded">
                                                {d.type}
                                            </span>
                                        </td>
                                        <td className="px-4 py-3">
                                            <span className="bg-gray-100 text-gray-700 text-xs font-semibold px-2 py-1 rounded">
                                                {d.event}
                                            </span>
                                        </td>
                                        <td className="px-4 py-3 text-gray-600">{d.officer}</td>
                                        <td className="px-4 py-3">
                                            <span className={`text-xs font-semibold px-3 py-1 rounded ${getSeverityBadge(d.severity)}`}>
                                                {d.severity}
                                            </span>
                                        </td>
                                        <td className="px-4 py-3 text-gray-600 max-w-xs truncate">{d.msg}</td>
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