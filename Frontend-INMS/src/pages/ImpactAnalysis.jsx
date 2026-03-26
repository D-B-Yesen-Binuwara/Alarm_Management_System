// pages/ImpactAnalysis.jsx
// import Navbar from "../components/Navbar";
import SummaryCard from "../components/SummaryCard";

export default function ImpactAnalysis() {
  return (
    <div className="bg-gray-100 min-h-screen">
      {/* <Navbar /> */}

      <div className="p-6 space-y-6">
        {/* Title */}
        <div>
          <h1 className="text-2xl font-semibold">
            Fault Propagation & Impact Analysis
          </h1>
          <p className="text-gray-500 text-sm">
            Identify affected downstream nodes and isolated network segments
          </p>
        </div>

        {/* Dropdown + Button */}
        <div className="bg-white p-4 rounded-lg flex gap-4 items-center shadow">
          <select className="border p-2 rounded w-full">
            <option>CEAN-BLR-001 (CEAN - reg3)</option>
          </select>
          <button className="bg-blue-500 text-white px-4 py-2 rounded">
            Run Impact Analysis
          </button>
        </div>

        {/* Summary */}
        <div className="grid grid-cols-4 gap-4">
          <SummaryCard title="SLBN Nodes Affected" value="0" color="bg-blue-50 border-blue-300" />
          <SummaryCard title="CEAN Nodes Affected" value="0" color="bg-purple-50 border-purple-300" />
          <SummaryCard title="MSAN Nodes Affected" value="2" color="bg-green-50 border-green-300" />
          <SummaryCard title="Total Nodes Affected" value="2" color="bg-orange-50 border-orange-300" />
        </div>

        {/* Isolated Segment */}
        <div className="bg-white p-4 rounded shadow">
          <h2 className="font-semibold mb-2">Isolated Network Segments</h2>
          <div className="bg-yellow-100 border-l-4 border-yellow-500 p-3 text-sm">
            reg3: 2 nodes affected
          </div>
        </div>

        {/* Table */}
        <div className="bg-white p-4 rounded shadow">
          <h2 className="font-semibold mb-4">Affected Nodes (2)</h2>

          <table className="w-full text-sm">
            <thead className="text-gray-500">
              <tr className="border-b">
                <th className="text-left p-2">Node Name</th>
                <th className="text-left p-2">IP Address</th>
                <th className="text-left p-2">Type</th>
                <th className="text-left p-2">Region</th>
                <th className="text-left p-2">Location</th>
                <th className="text-left p-2">Status</th>
              </tr>
            </thead>
            <tbody>
              <tr className="border-b">
                <td className="p-2">MSAN-BLR-001</td>
                <td className="p-2">10.3.3.1</td>
                <td className="p-2">MSAN</td>
                <td className="p-2">reg3</td>
                <td className="p-2">13.0569, 77.5937</td>
                <td className="p-2 text-green-600 font-semibold">UP</td>
              </tr>
              <tr>
                <td className="p-2">MSAN-BLR-002</td>
                <td className="p-2">10.3.3.2</td>
                <td className="p-2">MSAN</td>
                <td className="p-2">reg3</td>
                <td className="p-2">13.0210, 77.6382</td>
                <td className="p-2 text-green-600 font-semibold">UP</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}