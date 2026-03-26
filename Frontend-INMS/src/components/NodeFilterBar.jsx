// components/NodeFilterBar.jsx

const DEVICE_TYPES = ["SLBN", "CEAN", "MSAN", "Customer"];
const STATUSES = ["UP", "DOWN", "UNREACHABLE"];
const REGIONS = ["Metro", "region01", "region02", "region03"];

export default function NodeFilterBar({ filters, onChange }) {
  const input = "border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400";

  return (
    <div className="flex flex-wrap gap-3 mb-4">
      <input
        type="text"
        value={filters.search}
        onChange={(e) => onChange({ ...filters, search: e.target.value })}
        placeholder="Search by name or IP..."
        className={`${input} flex-1 min-w-48`}
      />

      <select
        value={filters.region}
        onChange={(e) => onChange({ ...filters, region: e.target.value })}
        className={input}
      >
        <option value="">All Regions</option>
        {REGIONS.map((r) => <option key={r} value={r}>{r}</option>)}
      </select>

      <select
        value={filters.type}
        onChange={(e) => onChange({ ...filters, type: e.target.value })}
        className={input}
      >
        <option value="">All Types</option>
        {DEVICE_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
      </select>

      <select
        value={filters.status}
        onChange={(e) => onChange({ ...filters, status: e.target.value })}
        className={input}
      >
        <option value="">All Status</option>
        {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
      </select>
    </div>
  );
}
