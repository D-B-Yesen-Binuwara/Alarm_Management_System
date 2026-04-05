import { useEffect, useState } from "react";

const EventPage = () => {
    const [events, setEvents] = useState([]);
    const [search, setSearch] = useState("");
    const [severityFilter, setSeverityFilter] = useState("ALL");

    // ✅ FIRST DEFINE FUNCTION
    const fetchEvents = async () => {
        try {
            const res = await fetch("http://localhost:7257/api/SimulationEvent");
            const data = await res.json();
            console.log(data); // 🔥 check data
            setEvents(data);
        } catch (err) {
            console.error("Error fetching events:", err);
        }
    };

    // ✅ THEN USE
    useEffect(() => {
        fetchEvents();
    }, []);

    // FILTER
    const filteredEvents = events.filter((e) => {
        const matchSearch =
            e.node?.toLowerCase().includes(search.toLowerCase()) ||
            e.message?.toLowerCase().includes(search.toLowerCase());

        const matchSeverity =
            severityFilter === "ALL" || e.severity === severityFilter;

        return matchSearch && matchSeverity;
    });

    return (
        <div className="p-6">
            <h2 className="text-xl font-bold mb-4">System Event Log</h2>

            <input
                type="text"
                placeholder="Search..."
                className="border p-2 mb-3"
                onChange={(e) => setSearch(e.target.value)}
            />

            <table className="w-full border">
                <thead>
                    <tr>
                        <th>Timestamp</th>
                        <th>Node</th>
                        <th>Event Type</th>
                        <th>Severity</th>
                        <th>Message</th>
                    </tr>
                </thead>

                <tbody>
                    {filteredEvents.map((e, i) => (
                        <tr key={i}>
                            <td>{e.timestamp}</td>
                            <td>{e.node}</td>
                            <td>{e.eventType}</td>
                            <td>{e.severity}</td>
                            <td>{e.message}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};

export default EventPage;