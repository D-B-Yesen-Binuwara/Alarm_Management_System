import { useState, useEffect, useMemo } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import DeviceService from '../services/DeviceService';
import AlarmService from '../services/AlarmService';
import {
  getDeviceTypeLabel,
  getPriorityLabel,
  normalizeStatus,
  isUpStatus,
  isDownStatus,
  formatDate,
  getAlarmRowClass,
  getAlarmBadgeClass,
  getStatusBadgeClass,
  getTypeBadgeClass,
  getPriorityBadgeClass
} from '../utils/formatters';

const Dashboard = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  
  const [devices, setDevices] = useState([]);
  const [alarms, setAlarms] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [lastRefreshed, setLastRefreshed] = useState(new Date());

  // Filters
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedType, setSelectedType] = useState('');
  const [selectedStatus, setSelectedStatus] = useState('');

  const availableDeviceTypes = useMemo(
    () => [...new Set(devices.map(d => getDeviceTypeLabel(d.deviceType)).filter(Boolean))],
    [devices]
  );

  const availableStatuses = useMemo(
    () => [...new Set(devices.map(d => normalizeStatus(d.status)).filter(Boolean))],
    [devices]
  );

  useEffect(() => {
    load();
  }, []);

  const load = async () => {
    setLoading(true);
    setError(null);

    try {
      const [devicesData, alarmsData] = await Promise.all([
        DeviceService.getAll(),
        AlarmService.getAll()
      ]);

      setDevices(devicesData);
      setAlarms(alarmsData);
      setLastRefreshed(new Date());
    } catch (err) {
      setError('Failed to load devices. Is the API running?');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const refresh = () => {
    load();
  };

  // Calculate metrics
  const totalNodes = devices.length;
  const activeNodes = devices.filter(d => isUpStatus(d.status)).length;
  const failedNodes = devices.filter(d => isDownStatus(d.status)).length;
  const activeAlarms = alarms.filter(a => a.isActive).length;

  // Get recent alarms
  const recentAlarms = alarms
    .filter(a => a.isActive)
    .sort((a, b) => new Date(b.raisedTime).getTime() - new Date(a.raisedTime).getTime())
    .slice(0, 5);

  // Filter devices based on search and filters
  const filteredDevices = devices.filter(d => {
    const term = searchTerm.toLowerCase();
    const matchesSearch =
      !term ||
      d.deviceName?.toLowerCase().includes(term) ||
      d.ip?.toLowerCase().includes(term);
    const matchesType = !selectedType || getDeviceTypeLabel(d.deviceType) === selectedType;
    const matchesStatus = !selectedStatus || normalizeStatus(d.status) === selectedStatus;
    return matchesSearch && matchesType && matchesStatus;
  });

  // Helper functions
  const getDeviceNameForAlarm = (deviceId) => {
    return devices.find(d => d.deviceId === deviceId)?.deviceName ?? `Device #${deviceId}`;
  };

  const hasAlarms = (deviceId) => {
    return alarms.some(a => a.deviceId === deviceId && a.isActive);
  };

  const formatTime = (dateStr) => {
    const d = new Date(dateStr);
    return d.toLocaleString('en-GB', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit'
    });
  };

  return (
    <div className="min-h-screen bg-gray-50 p-6">
      {/* Loading */}
      {loading && (
        <div className="flex items-center justify-center py-20 text-gray-500">
          <svg className="animate-spin h-6 w-6 mr-3 text-green-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
          </svg>
          Loading network data...
        </div>
      )}

      {/* Error */}
      {error && (
        <div className="mb-4 bg-red-50 border border-red-300 text-red-700 rounded-lg px-4 py-3 text-sm">
          ⚠️ {error}
        </div>
      )}

      {!loading && (
        <>
          {/* Summary Cards */}
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4 mb-6">
            {/* Total Nodes */}
            <div className="bg-white rounded-xl border border-gray-200 shadow-sm px-5 py-4 flex items-center gap-4">
              <div className="text-3xl">🌐</div>
              <div>
                <div className="text-3xl font-bold text-gray-800">{totalNodes}</div>
                <div className="text-sm text-gray-500">Total Nodes</div>
              </div>
            </div>

            {/* Active Nodes */}
            <div className="bg-white rounded-xl border border-green-300 shadow-sm px-5 py-4 flex items-center gap-4">
              <div className="text-3xl text-green-500">✅</div>
              <div>
                <div className="text-3xl font-bold text-gray-800">{activeNodes}</div>
                <div className="text-sm text-gray-500">Active Nodes</div>
              </div>
            </div>

            {/* Down Nodes */}
            <div className="bg-white rounded-xl border border-red-300 shadow-sm px-5 py-4 flex items-center gap-4">
              <div className="text-3xl text-red-500">❌</div>
              <div>
                <div className="text-3xl font-bold text-gray-800">{failedNodes}</div>
                <div className="text-sm text-gray-500">Down Nodes</div>
              </div>
            </div>

            {/* Active Alarms */}
            <div className="bg-white rounded-xl border border-yellow-300 shadow-sm px-5 py-4 flex items-center gap-4">
              <div className="text-3xl">🔔</div>
              <div>
                <div className="text-3xl font-bold text-gray-800">{activeAlarms}</div>
                <div className="text-sm text-gray-500">Active Alarms</div>
              </div>
            </div>
          </div>

          {/* Recent Alarms */}
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-5 mb-6">
            <h2 className="text-base font-semibold text-gray-700 mb-3">Recent Alarms</h2>

            {recentAlarms.length === 0 ? (
              <div className="text-sm text-gray-400 text-center py-6 border border-dashed border-gray-200 rounded-lg">
                No active alarms at this time.
              </div>
            ) : (
              <div className="space-y-2">
                {recentAlarms.map((alarm) => (
                  <div
                    key={alarm.alarmId}
                    className={`${getAlarmRowClass(alarm.alarmType)} rounded-lg px-4 py-3 flex justify-between items-start`}
                  >
                    <div>
                      <span className={`${getAlarmBadgeClass(alarm.alarmType)} text-xs font-semibold px-2 py-0.5 rounded mr-2`}>
                        {alarm.alarmType}
                      </span>
                      <span className="text-sm text-gray-700">
                        {alarm.alarmType} at {getDeviceNameForAlarm(alarm.deviceId)}
                      </span>
                    </div>
                    <span className="text-xs text-gray-400 whitespace-nowrap ml-4">
                      {formatDate(alarm.raisedTime)}
                    </span>
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Network Nodes Table */}
          <div className="bg-white rounded-xl border border-gray-200 shadow-sm p-5">
            <div className="flex items-center justify-between mb-4">
              <h2 className="text-base font-semibold text-gray-700">Network Nodes</h2>
              <button
                onClick={refresh}
                className="bg-blue-600 hover:bg-blue-700 text-white text-sm px-4 py-1.5 rounded-lg transition font-medium"
              >
                ↻ Refresh
              </button>
            </div>

            {/* Filters */}
            <div className="flex flex-wrap gap-3 mb-4">
              <input
                type="text"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                placeholder="Search by name or IP..."
                className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm flex-1 min-w-48 focus:outline-none focus:ring-2 focus:ring-green-400"
              />
              <select
                value={selectedType}
                onChange={(e) => setSelectedType(e.target.value)}
                className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-green-400"
              >
                <option value="">All Types</option>
                {availableDeviceTypes.map(type => (
                  <option key={type} value={type}>{type}</option>
                ))}
              </select>
              <select
                value={selectedStatus}
                onChange={(e) => setSelectedStatus(e.target.value)}
                className="border border-gray-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-green-400"
              >
                <option value="">All Status</option>
                {availableStatuses.map(s => (
                  <option key={s} value={s}>{s}</option>
                ))}
              </select>
            </div>

            {/* Table */}
            {filteredDevices.length === 0 ? (
              <div className="text-center text-gray-400 py-10 text-sm border border-dashed border-gray-200 rounded-lg">
                No devices match your filters.
              </div>
            ) : (
              <>
                <div className="overflow-x-auto">
                  <table className="w-full text-sm text-left">
                    <thead>
                      <tr className="border-b border-gray-200 text-gray-500 uppercase text-xs">
                        <th className="py-2 px-3 font-semibold">Name</th>
                        <th className="py-2 px-3 font-semibold">IP Address</th>
                        <th className="py-2 px-3 font-semibold">Type</th>
                        <th className="py-2 px-3 font-semibold">LEA ID</th>
                        <th className="py-2 px-3 font-semibold">Status</th>
                        <th className="py-2 px-3 font-semibold">Priority</th>
                        <th className="py-2 px-3 font-semibold">Alarms</th>
                        <th className="py-2 px-3 font-semibold">Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {filteredDevices.map((device) => (
                        <tr key={device.deviceId} className="border-b border-gray-100 hover:bg-gray-50 transition">
                          <td className="py-2.5 px-3 font-medium text-gray-800">{device.deviceName}</td>
                          <td className="py-2.5 px-3 text-gray-600 font-mono text-xs">{device.ip}</td>
                          <td className="py-2.5 px-3">
                            <span className={`${getTypeBadgeClass(device.deviceType)} text-xs font-semibold px-2 py-0.5 rounded`}>
                              {getDeviceTypeLabel(device.deviceType)}
                            </span>
                          </td>
                          <td className="py-2.5 px-3 text-gray-500">{device.leaId}</td>
                          <td className="py-2.5 px-3">
                            <span className={`${getStatusBadgeClass(device.status)} text-xs font-bold px-2 py-0.5 rounded`}>
                              {normalizeStatus(device.status)}
                            </span>
                          </td>
                          <td className="py-2.5 px-3">
                            <span className={`${getPriorityBadgeClass(device.priorityLevel)} text-xs font-semibold px-2 py-0.5 rounded`}>
                              {getPriorityLabel(device.priorityLevel)}
                            </span>
                          </td>
                          <td className="py-2.5 px-3">
                            <span className={`${hasAlarms(device.deviceId) ? 'bg-red-100 text-red-700' : 'bg-green-100 text-green-700'} text-xs font-semibold px-2 py-0.5 rounded`}>
                              {hasAlarms(device.deviceId) ? 'Failed' : 'Good'}
                            </span>
                          </td>
                          <td className="py-2.5 px-3">
                            <button
                              onClick={() => navigate(`/impact-analysis?deviceId=${device.deviceId}`)}
                              className="text-blue-600 hover:underline text-xs font-medium"
                            >
                              Analyze Impact
                            </button>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
                <div className="text-xs text-gray-400 mt-3">
                  Showing {filteredDevices.length} of {totalNodes} nodes &nbsp;·&nbsp;
                  Last refreshed: {formatTime(lastRefreshed.toISOString())}
                </div>
              </>
            )}
          </div>
        </>
      )}
    </div>
  );
};

export default Dashboard;
