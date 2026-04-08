const DEVICE_TABS = ['SLBN', 'CEAN', 'MSAN', 'Customer'];

export default function DeviceManagementFilter({ search, selectedType, onSearchChange, onTypeChange }) {
  const inputClass = 'w-full pl-10 pr-3 py-2 border border-slate-300 rounded-lg text-sm bg-slate-50 focus:outline-none focus:border-indigo-500 focus:bg-white focus:ring-2 focus:ring-indigo-500/10';

  return (
    <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-4 space-y-4">
      <div className="relative">
        <svg
          className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400"
          fill="none"
          stroke="currentColor"
          strokeWidth="2"
          strokeLinecap="round"
          strokeLinejoin="round"
          viewBox="0 0 24 24"
          aria-hidden="true"
        >
          <circle cx="11" cy="11" r="8" />
          <line x1="21" y1="21" x2="16.65" y2="16.65" />
        </svg>
        <input
          type="text"
          value={search}
          onChange={(event) => onSearchChange(event.target.value)}
          placeholder="Search by IP, Device Name, or Assigned Operator"
          className={inputClass}
        />
      </div>

      <div className="flex flex-wrap items-center gap-2">
        <button
          type="button"
          onClick={() => onTypeChange('')}
          className={`px-3 py-1.5 rounded-lg text-sm font-medium transition ${
            !selectedType
              ? 'bg-indigo-600 text-white shadow'
              : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
          }`}
        >
          All
        </button>

        {DEVICE_TABS.map((tab) => (
          <button
            key={tab}
            type="button"
            onClick={() => onTypeChange(tab)}
            className={`px-3 py-1.5 rounded-lg text-sm font-medium transition ${
              selectedType === tab
                ? 'bg-indigo-600 text-white shadow'
                : 'bg-slate-100 text-slate-600 hover:bg-slate-200'
            }`}
          >
            {tab}
          </button>
        ))}
      </div>
    </div>
  );
}
