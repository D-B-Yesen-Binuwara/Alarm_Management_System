// pages/ImpactAnalysis.jsx
import { useState, useEffect, useCallback } from "react";
import { useSearchParams } from "react-router-dom";
import SummaryCard from "../components/SummaryCard";
import NodeFilterBar from "../components/NodeFilterBar";
import DeviceService from "../services/DeviceService";
import ImpactAnalysisService from "../services/ImpactAnalysisService";
import {
  getStatusBadgeClass,
  getTypeBadgeClass,
  normalizeStatus
} from "../utils/formatters";

// Default filter values
const DEFAULT_FILTERS = { search: "", region: "", type: "", status: "" };

// Main ImpactAnalysis component
export default function ImpactAnalysis() {
  const [searchParams] = useSearchParams();
  const [selectedDeviceId, setSelectedDeviceId] = useState(null);
  const [nodeSearch, setNodeSearch] = useState("");
  const [filters, setFilters] = useState(DEFAULT_FILTERS);
  const [devices, setDevices] = useState([]);
  const [filteredDevices, setFilteredDevices] = useState([]);
  const [showDeviceDropdown, setShowDeviceDropdown] = useState(false);
  
  // State for impact analysis data
  const [impactData, setImpactData] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [analyzing, setAnalyzing] = useState(false);

  // Load all devices for search
  useEffect(() => {
    DeviceService.getAll()
      .then(setDevices)
      .catch(err => console.error("Failed to load devices:", err));
  }, []);

  // Load device from URL parameter
  useEffect(() => {
    const deviceId = searchParams.get("deviceId");
    if (!deviceId) return;

    const id = Number(deviceId);
    setSelectedDeviceId(id);
    
    // Find device name from loaded devices
    const device = devices.find(d => d.deviceId === id);
    if (device) {
      setNodeSearch(device.deviceName);
    } else {
      // If devices not loaded yet, fetch the specific device
      DeviceService.getById(id)
        .then((device) => {
          if (device) setNodeSearch(device.deviceName);
        })
        .catch((err) => console.error("Failed to load device:", err));
    }
  }, [searchParams, devices]);

  // Filter devices based on search input
  useEffect(() => {
    if (!nodeSearch.trim()) {
      setFilteredDevices([]);
      setShowDeviceDropdown(false);
      return;
    }

    const filtered = devices.filter(device => 
      device.deviceName.toLowerCase().includes(nodeSearch.toLowerCase()) ||
      device.deviceId.toString().includes(nodeSearch)
    ).slice(0, 10); // Limit to 10 results

    setFilteredDevices(filtered);
    setShowDeviceDropdown(filtered.length > 0);
  }, [nodeSearch, devices]);

  // Handle device selection
  const handleDeviceSelect = (device) => {
    setSelectedDeviceId(device.deviceId);
    setNodeSearch(device.deviceName);
    setShowDeviceDropdown(false);
  };

  // Load impact analysis data when device is selected
  useEffect(() => {
    if (!selectedDeviceId) return;

    const fetchImpactData = async () => {
      setLoading(true);
      setError(null);
      
      try {
        const result = await ImpactAnalysisService.getImpactResult(selectedDeviceId);
        setImpactData(result);
      } catch (err) {
        if (err.response?.status === 404) {
          // Device not found or no impact data - this is normal
          setImpactData(null);
        } else {
          setError("Failed to load impact analysis data. Please try again.");
        }
      } finally {
        setLoading(false);
      }
    };

    fetchImpactData();
  }, [selectedDeviceId]);

  // Handle Run Analysis button click
  const handleRunAnalysis = useCallback(async () => {
    if (!selectedDeviceId) {
      setError("Please select a device first");
      return;
    }

    setAnalyzing(true);
    setError(null);
    
    try {
      const result = await ImpactAnalysisService.analyzeDeviceFailure(selectedDeviceId);
      setImpactData(result);
    } catch (err) {
      if (err.response) {
        setError(`Analysis failed: ${err.response.data?.error || err.response.statusText}`);
      } else if (err.request) {
        setError("Network error: Unable to connect to the server. Please check if the backend is running.");
      } else {
        setError(`Analysis failed: ${err.message}`);
      }
    } finally {
      setAnalyzing(false);
    }
  }, [selectedDeviceId]);

  // Process impacted devices from backend response
  const impactedDevices = impactData?.impactedDevices || [];
  const rootCause = impactData?.rootCause;
  const rootDevice = rootCause?.rootDevice;

  // Count devices by type
  const getDeviceTypeCounts = useCallback(() => {
    return impactedDevices.reduce((acc, device) => {
      const type = device.deviceType || "UNKNOWN";
      acc[type] = (acc[type] || 0) + 1;
      return acc;
    }, {});
  }, [impactedDevices]);

  const typeCounts = getDeviceTypeCounts();

  // Filter impacted devices based on search and filters
  const filtered = impactedDevices.filter((device) => {
    const term = filters.search.toLowerCase();
    const deviceName = device.deviceName || "";
    if (term && !deviceName.toLowerCase().includes(term)) return false;
    if (filters.status && device.status !== filters.status) return false;
    return true;
  });

  // Calculate affected count by region/segment
  const affectedCount = impactedDevices.length;

  return (
    <div className="min-h-screen bg-gray-50 p-6 space-y-6">
      {/* Page Title */}
      <div>
        <h1 className="text-2xl font-semibold text-gray-800">
          Fault Localization & Impact Analysis
        </h1>
        <p className="text-gray-500 text-sm mt-0.5">
          Identify affected nodes and isolated network segments
        </p>
      </div>

      {/* Error Alert */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <p className="text-red-800 text-sm font-medium">{error}</p>
        </div>
      )}

      {/* Root Cause Card */}
      {rootCause && rootDevice && (
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {/* Root Cause Device Info */}
          <div className="bg-red-50 border border-red-200 rounded-xl p-4">
            <h3 className="text-red-900 font-semibold text-sm mb-4">Root Cause Device</h3>
            <div className="space-y-2 text-sm">
              <div><strong className="text-red-700">Device:</strong> {rootDevice.deviceName}</div>
              <div><strong className="text-red-700">Device ID:</strong> {rootDevice.deviceId}</div>
              <div><strong className="text-red-700">Type:</strong> <span className={`${getTypeBadgeClass(rootDevice.deviceType)} px-2 py-1 rounded text-xs font-semibold`}>{rootDevice.deviceType}</span></div>
              <div><strong className="text-red-700">Status:</strong> <span className={`${getStatusBadgeClass(rootDevice.status)} px-2 py-1 rounded text-xs font-bold`}>{rootDevice.status}</span></div>
              <div><strong className="text-red-700">IP Address:</strong> <span className="font-mono">{rootDevice.ip || 'N/A'}</span></div>
              <div><strong className="text-red-700">Coordinates:</strong> {rootDevice.latitude?.toFixed(4)}, {rootDevice.longitude?.toFixed(4)}</div>
              <div><strong className="text-red-700">LEA:</strong> {rootDevice.lea || 'N/A'}</div>
              <div><strong className="text-red-700">Province:</strong> {rootDevice.province || 'N/A'}</div>
              <div><strong className="text-red-700">Region:</strong> {rootDevice.region || 'N/A'}</div>
            </div>
          </div>

          {/* Impact Summary */}
          <div className="bg-orange-50 border border-orange-200 rounded-xl p-4">
            <h3 className="text-orange-900 font-semibold text-sm mb-4">Impact Summary</h3>
            <div className="space-y-2 text-sm">
              <div><strong className="text-orange-700">Root Cause Type:</strong> {rootCause.rootCauseType}</div>
              <div><strong className="text-orange-700">Detected Time:</strong> {new Date(rootCause.detectedTime).toLocaleString()}</div>
              <div><strong className="text-orange-700">Total Affected Devices:</strong> <span className="text-lg font-bold text-orange-700">{affectedCount}</span></div>
              {Object.entries(typeCounts).map(([type, count]) => (
                <div key={type}><strong className="text-orange-700">{type}:</strong> {count} device(s)</div>
              ))}
            </div>
          </div>
        </div>
      )}

      {/* Node Selector */}
      <div className="bg-white p-4 rounded-xl border border-gray-200 shadow-sm space-y-2">
        <label className="text-sm font-medium text-gray-600">
          Select Source Node
        </label>
        <div className="flex gap-2 relative">
          <div className="flex-1 relative">
            <input
              type="text"
              value={nodeSearch}
              onChange={(e) => setNodeSearch(e.target.value)}
              onFocus={() => nodeSearch && setShowDeviceDropdown(filteredDevices.length > 0)}
              placeholder="Search node by name or ID..."
              className="border border-gray-300 rounded-l-lg px-3 py-2 text-sm
              w-full focus:outline-none focus:ring-2 focus:ring-blue-400"
            />
            {showDeviceDropdown && (
              <div className="absolute top-full left-0 right-0 bg-white border border-gray-300 rounded-lg shadow-lg z-10 max-h-60 overflow-y-auto">
                {filteredDevices.map(device => (
                  <div
                    key={device.deviceId}
                    onClick={() => handleDeviceSelect(device)}
                    className="px-3 py-2 hover:bg-gray-100 cursor-pointer border-b border-gray-100 last:border-b-0"
                  >
                    <div className="font-medium text-sm">{device.deviceName}</div>
                    <div className="text-xs text-gray-500">ID: {device.deviceId} | Status: {device.status}</div>
                  </div>
                ))}
              </div>
            )}
          </div>
          <button
            onClick={handleRunAnalysis}
            disabled={!selectedDeviceId || analyzing}
            className="bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 text-white text-sm
            font-medium px-4 py-2 rounded-r-lg transition whitespace-nowrap"
          >
            {analyzing ? "Analyzing..." : "Run Analysis"}
          </button>
        </div>
        {selectedDeviceId && (
          <div className="text-xs text-green-600">
            Selected: Device ID {selectedDeviceId}
          </div>
        )}
      </div>

      {/* Loading State */}
      {loading && (
        <div className="flex items-center justify-center py-12">
          <div className="text-center">
            <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
            <p className="text-gray-600 text-sm mt-4">Loading impact analysis...</p>
          </div>
        </div>
      )}

      {/* Summary Cards */}
      {!loading && impactData && (
        <>
          <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
            <SummaryCard 
              title="Total Nodes Affected" 
              value={affectedCount.toString()} 
              color="bg-orange-50 border-orange-300" 
            />
            {Object.entries(typeCounts).map(([type, count]) => (
              <SummaryCard
                key={type}
                title={`${type} Affected`}
                value={count.toString()}
                color="bg-blue-50 border-blue-300"
              />
            ))}
          </div>

          {/* Affected Nodes Table */}
          <div className="bg-white p-4 rounded-xl border border-gray-200 shadow-sm">
            <h2 className="text-base font-semibold text-gray-700 mb-4">
              Affected Nodes ({affectedCount})
            </h2>

            <NodeFilterBar 
              filters={filters} 
              onChange={setFilters} 
              nodes={impactedDevices} 
            />

            {filtered.length === 0 ? (
              <div className="text-center text-gray-400 py-10 text-sm">
                {impactedDevices.length === 0 
                  ? "No affected nodes found." 
                  : "No nodes match the current filters."}
              </div>
            ) : (
              <div className="overflow-x-auto">
                <table className="w-full text-sm text-left">
                  <thead>
                    <tr className="border-b border-gray-200 text-gray-500 uppercase text-xs">
                      <th className="py-2 px-3 font-semibold">Device Name</th>
                      <th className="py-2 px-3 font-semibold">Type</th>
                      <th className="py-2 px-3 font-semibold">Status</th>
                      <th className="py-2 px-3 font-semibold">IP Address</th>
                      <th className="py-2 px-3 font-semibold">LEA</th>
                      <th className="py-2 px-3 font-semibold">Province</th>
                      <th className="py-2 px-3 font-semibold">Region</th>
                      <th className="py-2 px-3 font-semibold">Coordinates</th>
                      <th className="py-2 px-3 font-semibold">Impact Type</th>
                    </tr>
                  </thead>
                  <tbody>
                    {filtered.map((device) => (
                      <tr 
                        key={device.deviceId} 
                        className="border-b border-gray-100 hover:bg-gray-50 transition"
                      >
                        <td className="py-2.5 px-3 font-medium text-gray-800">
                          {device.deviceName}
                        </td>
                        <td className="py-2.5 px-3">
                          <span className={`${getTypeBadgeClass(device.deviceType)} text-xs font-semibold px-2 py-0.5 rounded`}>
                            {device.deviceType}
                          </span>
                        </td>
                        <td className="py-2.5 px-3">
                          <span className={`${getStatusBadgeClass(device.status)} text-xs font-bold px-2 py-0.5 rounded`}>
                            {device.status}
                          </span>
                        </td>
                        <td className="py-2.5 px-3 font-mono text-xs text-gray-600">
                          {device.ip || 'N/A'}
                        </td>
                        <td className="py-2.5 px-3 text-sm text-gray-700">
                          {device.lea || 'N/A'}
                        </td>
                        <td className="py-2.5 px-3 text-sm text-gray-700">
                          {device.province || 'N/A'}
                        </td>
                        <td className="py-2.5 px-3 text-sm text-gray-700">
                          {device.region || 'N/A'}
                        </td>
                        <td className="py-2.5 px-3 font-mono text-xs text-gray-600">
                          {device.latitude?.toFixed(4)}, {device.longitude?.toFixed(4)}
                        </td>
                        <td className="py-2.5 px-3 text-sm text-gray-700">
                          {device.impactType}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </>
      )}

      {/* Empty State */}
      {!loading && !impactData && !error && (
        <div className="bg-gray-100 rounded-lg p-12 text-center">
          <p className="text-gray-600 text-sm">
            Select a device and click "Run Analysis" to see impact analysis results
          </p>
        </div>
      )}
    </div>
  );
}
