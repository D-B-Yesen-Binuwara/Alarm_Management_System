/**
 * Device type labels mapping
 */
export const DEVICE_TYPE_LABELS = {
  0: 'SLBN',
  1: 'CEAN',
  2: 'MSAN',
  3: 'Customer',
  'SLBN': 'SLBN',
  'CEAN': 'CEAN',
  'MSAN': 'MSAN',
  'Customer': 'Customer'
};

/**
 * Priority labels mapping
 */
export const PRIORITY_LABELS = {
  1: 'Low',
  2: 'Avg',
  3: 'High',
  4: 'Critical',
  0: 'Low',
  'Low': 'Low',
  'Avg': 'Avg',
  'Medium': 'Medium',
  'High': 'High',
  'Critical': 'Critical'
};

/**
 * Get device type label
 * @param {string|number} deviceType
 * @returns {string}
 */
export const getDeviceTypeLabel = (deviceType) => {
  if (typeof deviceType === 'number') {
    return DEVICE_TYPE_LABELS[deviceType] ?? String(deviceType);
  }

  const normalized = String(deviceType ?? '').trim();
  return DEVICE_TYPE_LABELS[normalized] ?? normalized;
};

/**
 * Get priority label
 * @param {string|number} priority
 * @returns {string}
 */
export const getPriorityLabel = (priority) => {
  if (typeof priority === 'number') {
    return PRIORITY_LABELS[priority] ?? String(priority);
  }

  const normalized = String(priority ?? '').trim();
  return PRIORITY_LABELS[normalized] ?? normalized;
};

/**
 * Check if a status indicates UP/ONLINE
 * @param {string|number|null} status
 * @returns {boolean}
 */
export const isUpStatus = (status) => {
  const value = String(status ?? '').trim().toUpperCase();
  return value === 'UP' || value === 'ONLINE' || value === 'ALIVE' || value === '0' || value === 'TRUE';
};

/**
 * Check if a status indicates DOWN/OFFLINE
 * @param {string|number|null} status
 * @returns {boolean}
 */
export const isDownStatus = (status) => {
  const value = String(status ?? '').trim().toUpperCase();
  return value === 'DOWN' || value === 'OFFLINE' || value === 'DEAD' || value === 'FAILED' || value === '1' || value === '2' || value === 'UNREACHABLE' || value === 'FALSE';
};

/**
 * Normalize status to UP or DOWN
 * @param {string|number|null} status
 * @returns {string}
 */
export const normalizeStatus = (status) => {
  const normalized = String(status ?? '').trim().toUpperCase();

  if (normalized === '2' || normalized === 'UNREACHABLE') {
    return 'UNREACHABLE';
  }
  
  if (isUpStatus(normalized)) {
    return 'UP';
  }
  
  if (isDownStatus(normalized)) {
    return 'DOWN';
  }
  
  return normalized;
};

/**
 * Format date to readable string
 * @param {string} dateStr - ISO date string
 * @returns {string}
 */
export const formatDate = (dateStr) => {
  const d = new Date(dateStr);
  return d.toLocaleString('en-GB', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  });
};

/**
 * Format time only
 * @param {string} dateStr - ISO date string
 * @returns {string}
 */
export const formatTime = (dateStr) => {
  const d = new Date(dateStr);
  return d.toLocaleString('en-GB', {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  });
};

/**
 * Get CSS badge class for alarm type
 * @param {string} alarmType
 * @returns {string}
 */
export const getAlarmBadgeClass = (alarmType) => {
  const t = alarmType?.toUpperCase();
  if (t === 'NODE_DOWN') return 'bg-red-200 text-red-800';
  if (t === 'AC') return 'bg-orange-200 text-orange-800';
  return 'bg-yellow-200 text-yellow-800';
};

/**
 * Get CSS row class for alarm
 * @param {string} alarmType
 * @returns {string}
 */
export const getAlarmRowClass = (alarmType) => {
  const t = alarmType?.toUpperCase();
  if (t === 'NODE_DOWN') return 'bg-red-50 border-l-4 border-red-400';
  if (t === 'AC') return 'bg-orange-50 border-l-4 border-orange-400';
  return 'bg-yellow-50 border-l-4 border-yellow-400';
};

/**
 * Get CSS badge class for device status
 * @param {string} status
 * @returns {string}
 */
export const getStatusBadgeClass = (status) => {
  const normalized = normalizeStatus(status);

  if (normalized === 'UNREACHABLE') {
    return 'bg-yellow-100 text-yellow-800';
  }

  return isUpStatus(status)
    ? 'bg-green-100 text-green-800'
    : 'bg-red-100 text-red-800';
};

/**
 * Get CSS badge class for device type
 * @param {string|number} deviceType
 * @returns {string}
 */
export const getTypeBadgeClass = (deviceType) => {
  const label = getDeviceTypeLabel(deviceType);
  const map = {
    'SLBN': 'bg-purple-100 text-purple-800',
    'CEAN': 'bg-blue-100 text-blue-800',
    'MSAN': 'bg-teal-100 text-teal-800',
    'Customer': 'bg-gray-100 text-gray-800'
  };
  return map[label] ?? 'bg-gray-100 text-gray-800';
};

/**
 * Get CSS badge class for priority level
 * @param {string|number} priority
 * @returns {string}
 */
export const getPriorityBadgeClass = (priority) => {
  const label = getPriorityLabel(priority);
  const map = {
    'Critical': 'bg-red-100 text-red-800',
    'High': 'bg-orange-100 text-orange-800',
    'Avg': 'bg-yellow-100 text-yellow-800',
    'Medium': 'bg-yellow-100 text-yellow-800',
    'Low': 'bg-green-100 text-green-800'
  };
  return map[label] ?? 'bg-gray-100 text-gray-700';
};
