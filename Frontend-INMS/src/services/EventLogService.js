import apiClient from './apiClient';

/**
 * Event Log Service - handles simulation event API calls
 */
class EventLogService {
  /**
   * Get all event log entries
   * @returns {Promise<Array>}
   */
  static async getAll() {
    try {
      const response = await apiClient.get('/simulationevent');
      return response.data;
    } catch (error) {
      console.error('Failed to fetch event logs:', error);
      throw error;
    }
  }

  /**
   * Get event log entries for a specific device
   * @param {number} deviceId
   * @returns {Promise<Array>}
   */
  static async getByDevice(deviceId) {
    try {
      const response = await apiClient.get(`/simulationevent/device/${deviceId}`);
      return response.data;
    } catch (error) {
      console.error(`Failed to fetch event logs for device ${deviceId}:`, error);
      throw error;
    }
  }
}

export default EventLogService;
