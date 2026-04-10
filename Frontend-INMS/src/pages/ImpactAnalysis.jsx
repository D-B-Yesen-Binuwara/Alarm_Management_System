// pages/ImpactAnalysis.jsx
import { useState } from "react";
import SummaryCard from "../components/SummaryCard";
import NodeFilterBar from "../components/NodeFilterBar";
import {
  getStatusBadgeClass,
  getTypeBadgeClass,
  normalizeStatus
} from "../utils/formatters";

const MOCK_NODES = [
  {
    name: "MSAN-BLR-001",
    ip: "10.3.3.1",
    type: "MSAN",
    region: "reg3",
    province: "Karnataka",
    parent: "CEAN-BLR-001",
    childs: "—",
    location: "13.0569, 77.5937",
    status: "UP",
  },
  {
    name: "MSAN-BLR-002",
    ip: "10.3.3.2",
    type: "MSAN",
    region: "reg3",
    province: "Karnataka",
    parent: "CEAN-BLR-001",
    childs: "—",
    location: "13.0210, 77.6382",
    status: "UP",
  },
];

const DEFAULT_FILTERS = { search: "", region: "", type: "", status: "" };

export default function ImpactAnalysis() {
  const [filters, setFilters] = useState(DEFAULT_FILTERS);

  const filtered = MOCK_NODES.filter((n) => {
    const term = filters.search.toLowerCase();
    if (term && !n.name.toLowerCase().includes(term) && !n.ip.includes(term)) return false;
    if (filters.region && n.region !== filters.region) return false;
    if (filters.type && n.type !== filters.type) return false;
    if (filters.status && normalizeStatus(n.status) !== filters.status) return false;
    return true;
  });

  const affectedCount = MOCK_NODES.length;

  return (
    <div className="min-h-screen bg-gray-50 p-6 space-y-6">
      {/* Title */}
      <div>
        <h1 className="text-2xl font-semibold text-gray-800">
          Fault Localization & Impact Analysis
        </h1>
        <p className="text-gray-500 text-sm mt-0.5">
          Identify affected nodes and isolated network segments
        </p>
      </div>

      {/* Selector */}
      <div className="bg-white p-4 rounded-xl border border-gray-200 shadow-sm space-y-3">
        <label className="text-sm font-medium text-gray-600">
          Select Source Node
        </label>
        <div className="flex">
          <select className="border border-gray-300 rounded-l-lg px-3 py-2 text-sm flex-1 focus:outline-none focus:ring-2 focus:ring-blue-400">
            <option>CEAN-BLR-001 (CEAN — reg3)</option>
          </select>
          <button className="bg-blue-600 hover:bg-blue-700 text-white text-sm font-medium px-4 py-2 rounded-r-lg transition whitespace-nowrap">
            Run Analysis
          </button>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        <SummaryCard title="SLBN Nodes Affected" value="0" color="bg-purple-50 border-purple-300" />
        <SummaryCard title="CEAN Nodes Affected" value="0" color="bg-blue-50 border-blue-300" />
        <SummaryCard title="MSAN Nodes Affected" value="2" color="bg-teal-50 border-teal-300" />
        <SummaryCard title="Total Nodes Affected" value="2" color="bg-orange-50 border-orange-300" />
      </div>

      {/* Isolated Segments */}
      <div className="bg-white p-4 rounded-xl border border-gray-200 shadow-sm">
        <h2 className="text-base font-semibold text-gray-700 mb-3">
          Isolated Network Segments
        </h2>
        <div className="bg-yellow-50 border-l-4 border-yellow-400 rounded px-4 py-3 text-sm text-yellow-800">
          reg3: {affectedCount} nodes affected
        </div>
      </div>

      {/* Affected Nodes Table */}
      <div className="bg-white p-4 rounded-xl border border-gray-200 shadow-sm">
        <h2 className="text-base font-semibold text-gray-700 mb-4">
          Affected Nodes ({affectedCount})
        </h2>

        <NodeFilterBar filters={filters} onChange={setFilters} nodes={MOCK_NODES} />

        <div className="overflow-x-auto">
          <table className="w-full text-sm text-left">
            <thead>
              <tr className="border-b border-gray-200 text-gray-500 uppercase text-xs">
                <th className="py-2 px-3 font-semibold">Node Name</th>
                <th className="py-2 px-3 font-semibold">IP Address</th>
                <th className="py-2 px-3 font-semibold">Type</th>
                <th className="py-2 px-3 font-semibold">Region</th>
                <th className="py-2 px-3 font-semibold">Province</th>
                <th className="py-2 px-3 font-semibold">Parent</th>
                <th className="py-2 px-3 font-semibold">Childs</th>
                <th className="py-2 px-3 font-semibold">Location</th>
                <th className="py-2 px-3 font-semibold">Status</th>
              </tr>
            </thead>
            <tbody>
              {filtered.length === 0 ? (
                <tr>
                  <td colSpan={9} className="text-center text-gray-400 py-10 text-sm">
                    No affected nodes found.
                  </td>
                </tr>
              ) : (
                filtered.map((node) => (
                  <tr key={node.name} className="border-b border-gray-100 hover:bg-gray-50 transition">
                    <td className="py-2.5 px-3 font-medium text-gray-800">{node.name}</td>
                    <td className="py-2.5 px-3 text-gray-600 font-mono text-xs">{node.ip}</td>
                    <td className="py-2.5 px-3">
                      <span className={`${getTypeBadgeClass(node.type)} text-xs font-semibold px-2 py-0.5 rounded`}>
                        {node.type}
                      </span>
                    </td>
                    <td className="py-2.5 px-3 text-gray-600">{node.region}</td>
                    <td className="py-2.5 px-3 text-gray-600">{node.province}</td>
                    <td className="py-2.5 px-3 text-gray-500 text-xs">{node.parent}</td>
                    <td className="py-2.5 px-3 text-gray-500 text-xs">{node.childs}</td>
                    <td className="py-2.5 px-3 text-gray-500 text-xs">{node.location}</td>
                    <td className="py-2.5 px-3">
                      <span className={`${getStatusBadgeClass(node.status)} text-xs font-bold px-2 py-0.5 rounded`}>
                        {normalizeStatus(node.status)}
                      </span>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
