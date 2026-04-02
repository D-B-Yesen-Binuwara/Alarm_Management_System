/**
 * @typedef {Object} Alarm
 * @property {number} alarmId
 * @property {number} deviceId
 * @property {string} alarmType
 * @property {string} raisedTime
 * @property {string} [clearedTime]
 * @property {boolean} isActive
 */

// This file serves as documentation for the Alarm model
export const alarmModelSchema = {
  alarmId: 'number',
  deviceId: 'number',
  alarmType: 'string',
  raisedTime: 'string (ISO datetime)',
  clearedTime: 'string (ISO datetime, optional)',
  isActive: 'boolean'
};
