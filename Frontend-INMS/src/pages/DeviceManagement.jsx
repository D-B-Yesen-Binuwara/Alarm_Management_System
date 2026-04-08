import { useEffect, useMemo, useState } from 'react';
import DeviceService from '../services/DeviceService';
import DeviceManagementFilter from '../components/DeviceManagementFilter';
import DeviceCard from '../components/DeviceCard';
import DeviceFormModal from '../components/DeviceFormModal';
import DeviceDetailsModal from '../components/DeviceDetailsModal';
import { getDeviceTypeLabel } from '../utils/formatters';

function getAssignedOperatorName(device) {
  return (
    device.assignedUserName
    ?? device.operatorName
    ?? device.assignedOperator
    ?? device.userName
    ?? (device.assignedUserId != null ? `User #${device.assignedUserId}` : '-')
  );
}

function matchesSearch(device, searchTerm) {
  if (!searchTerm) {
    return true;
  }

  const term = searchTerm.toLowerCase();
  const assignedOperator = getAssignedOperatorName(device).toLowerCase();

  return (
    String(device.ip ?? '').toLowerCase().includes(term)
    || String(device.deviceName ?? '').toLowerCase().includes(term)
    || String(device.assignedUserId ?? '').toLowerCase().includes(term)
    || assignedOperator.includes(term)
  );
}

function mapById(devices) {
  const map = new Map();
  devices.forEach((device) => {
    if (device.deviceId != null) {
      map.set(device.deviceId, device);
    }
  });
  return map;
}

export default function DeviceManagement() {
  const [devices, setDevices] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [search, setSearch] = useState('');
  const [selectedType, setSelectedType] = useState('');

  const [activeDevice, setActiveDevice] = useState(null);
  const [formMode, setFormMode] = useState('add');
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDetailsOpen, setIsDetailsOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  const loadDevices = async () => {
    setLoading(true);
    setError('');

    try {
      const data = await DeviceService.getAll();
      setDevices(Array.isArray(data) ? data : []);
    } catch (requestError) {
      setError('Failed to load devices. Is the API running?');
      console.error(requestError);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDevices();
  }, []);

  const devicesById = useMemo(() => mapById(devices), [devices]);

  const filteredDevices = useMemo(() => {
    return devices.filter((device) => {
      const typeLabel = getDeviceTypeLabel(device.deviceType);
      const matchesType = !selectedType || typeLabel === selectedType;
      return matchesType && matchesSearch(device, search);
    });
  }, [devices, search, selectedType]);

  const openAddModal = () => {
    setFormMode('add');
    setActiveDevice(null);
    setIsFormOpen(true);
  };

  const openEditModal = (device) => {
    setFormMode('edit');
    setActiveDevice(device);
    setIsDetailsOpen(false);
    setIsFormOpen(true);
  };

  const openDetails = (device) => {
    setActiveDevice(device);
    setIsDetailsOpen(true);
  };

  const closeDetails = () => {
    setIsDetailsOpen(false);
  };

  const closeForm = () => {
    if (!submitting) {
      setIsFormOpen(false);
    }
  };

  const handleSubmitForm = async (payload) => {
    setSubmitting(true);

    try {
      if (formMode === 'add') {
        const created = await DeviceService.create(payload);

        setDevices((previous) => {
          const next = Array.isArray(previous) ? [...previous] : [];
          if (created?.deviceId != null) {
            const index = next.findIndex((item) => item.deviceId === created.deviceId);
            if (index >= 0) {
              next[index] = created;
            } else {
              next.unshift(created);
            }
            return next;
          }

          return next;
        });
      } else if (activeDevice?.deviceId != null) {
        const updated = await DeviceService.update(activeDevice.deviceId, payload);

        setDevices((previous) => previous.map((device) => (
          device.deviceId === activeDevice.deviceId
            ? { ...device, ...updated, ...payload }
            : device
        )));
      }

      setIsFormOpen(false);

      if (formMode === 'edit' && activeDevice?.deviceId != null) {
        const latest = devicesById.get(activeDevice.deviceId);
        if (latest) {
          setActiveDevice({ ...latest, ...payload });
        }
      }
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="max-w-7xl mx-auto flex flex-col gap-6">
      <header className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">Device Management</h1>
          <p className="text-sm text-slate-500 mt-1">Monitor, filter, review, and maintain network devices.</p>
        </div>
        <button
          type="button"
          onClick={openAddModal}
          className="inline-flex items-center justify-center gap-2 px-4 py-2 rounded-lg text-sm font-medium text-white bg-gradient-to-r from-indigo-600 to-blue-500 hover:from-indigo-700 hover:to-blue-600 shadow-md transition"
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24" aria-hidden="true">
            <line x1="12" y1="5" x2="12" y2="19" />
            <line x1="5" y1="12" x2="19" y2="12" />
          </svg>
          Add New Device
        </button>
      </header>

      {error && (
        <div className="px-4 py-3 rounded-lg text-sm font-medium bg-red-50 text-red-800 border border-red-200">
          {error}
        </div>
      )}

      <DeviceManagementFilter
        search={search}
        selectedType={selectedType}
        onSearchChange={setSearch}
        onTypeChange={setSelectedType}
      />

      {loading ? (
        <div className="flex items-center justify-center py-20 text-slate-500 bg-white rounded-xl border border-slate-200">
          <svg className="animate-spin h-6 w-6 mr-3 text-indigo-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
          </svg>
          Loading devices...
        </div>
      ) : filteredDevices.length === 0 ? (
        <div className="text-center text-slate-400 py-12 text-sm border border-dashed border-slate-300 rounded-xl bg-white">
          No devices match your current filters.
        </div>
      ) : (
        <div className="grid grid-cols-1 xl:grid-cols-2 gap-4">
          {filteredDevices.map((device) => (
            <DeviceCard
              key={device.deviceId ?? `${device.deviceName}-${device.ip}`}
              device={device}
              assignedOperatorName={getAssignedOperatorName(device)}
              onView={openDetails}
              onEdit={openEditModal}
            />
          ))}
        </div>
      )}

      {isDetailsOpen && activeDevice && (
        <DeviceDetailsModal
          device={activeDevice}
          assignedOperatorName={getAssignedOperatorName(activeDevice)}
          onClose={closeDetails}
          onEdit={openEditModal}
        />
      )}

      {isFormOpen && (
        <DeviceFormModal
          mode={formMode}
          initialDevice={formMode === 'edit' ? activeDevice : null}
          onClose={closeForm}
          onSubmit={handleSubmitForm}
          submitting={submitting}
        />
      )}
    </div>
  );
}
