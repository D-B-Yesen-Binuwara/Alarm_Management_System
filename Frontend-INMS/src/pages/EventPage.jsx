import React, { useState } from "react";

const EventPage = () => {
    const [search, setSearch] = useState("");
    const [severity, setSeverity] = useState("All");
    const [type, setType] = useState("All");

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

    // 🔍 FILTER
    const filtered = data.filter((d) => {
        return (
            (d.node.toLowerCase().includes(search.toLowerCase()) ||
                d.msg.toLowerCase().includes(search.toLowerCase())) &&
            (severity === "All" || d.severity === severity) &&
            (type === "All" || d.type === type)
        );
    });

    // 🎨 ROW COLORS
    const getRowColor = (sev) => {
        if (sev === "CRITICAL") return "#f8d7da";
        if (sev === "MAJOR") return "#fff3cd";
        if (sev === "INFO") return "#d1ecf1";
    };

    // 🎨 BADGE SAME SIZE
    const badge = (sev) => {
        let bg = "gray";
        if (sev === "CRITICAL") bg = "red";
        if (sev === "MAJOR") bg = "orange";
        if (sev === "INFO") bg = "blue";

        return {
            background: bg,
            color: "white",
            padding: "4px 0",
            borderRadius: "4px",
            fontSize: "11px",
            width: "80px",
            display: "inline-block",
            textAlign: "center"
        };
    };

    return (
        <div style={{ padding: "20px", background: "#f5f5f5", minHeight: "100vh" }}>

            <h3>System Event Log</h3>

            {/* 🔥 FULL FILTER BAR */}
            <div style={{ display: "flex", gap: "10px", marginBottom: "10px" }}>

                {/* TYPE FILTER */}
                <select onChange={(e) => setType(e.target.value)}>
                    {types.map((t, i) => (
                        <option key={i}>{t}</option>
                    ))}
                </select>

                {/* SEVERITY FILTER */}
                <select onChange={(e) => setSeverity(e.target.value)}>
                    <option>All</option>
                    <option>CRITICAL</option>
                    <option>MAJOR</option>
                    <option>INFO</option>
                </select>

                {/* SEARCH */}
                <input
                    placeholder="Search events..."
                    onChange={(e) => setSearch(e.target.value)}
                />

                <button style={{ marginLeft: "auto", background: "#2196F3", color: "white", border: "none", padding: "5px 10px" }}>
                    Refresh
                </button>
            </div>

            {/* 📊 TABLE */}
            <table style={{ width: "100%", background: "white", borderCollapse: "collapse" }}>
                <thead style={{ background: "#eee" }}>
                    <tr>
                        <th>Timestamp</th>
                        <th>Node</th>
                        <th>Type</th>
                        <th>Event Type</th>
                        <th>Officer</th>
                        <th>Severity</th>
                        <th>Message</th>
                    </tr>
                </thead>

                <tbody>
                    {filtered.map((d, i) => (
                        <tr key={i} style={{ background: getRowColor(d.severity) }}>
                            <td>{d.time}</td>
                            <td>{d.node}</td>
                            <td>{d.type}</td>
                            <td>{d.event}</td>
                            <td>{d.officer}</td>
                            <td>
                                <span style={badge(d.severity)}>
                                    {d.severity}
                                </span>
                            </td>
                            <td>{d.msg}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default EventPage;