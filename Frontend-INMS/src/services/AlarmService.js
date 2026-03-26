import apiClient from './apiClient';

/**
 * Alarm Service - handles all alarm-related API calls
 */
class AlarmService {
  /**
   * Get all alarms
   * @returns {Promise<Array>}
   */
  static async getAll() {
    try {
      const response = await apiClient.get('/alarm');
      return response.data;
    } catch (error) {
      console.error('Failed to fetch all alarms:', error);
      return [];
    }
  }

  /**
   * Get active alarms
   * @returns {Promise<Array>}
   */
  static async getActive() {
    try {
      const response = await apiClient.get('/alarm/active');
      return response.data;
    } catch (error) {
      console.error('Failed to fetch active alarms:', error);
      return [];
    }
  }

  /**
   * Get alarms for a specific device
   * @param {number} deviceId
   * @returns {Promise<Array>}
   */
  static async getByDevice(deviceId) {
    try {
      const response = await apiClient.get(`/alarm/device/${deviceId}`);
      return response.data;
    } catch (error) {
      console.error(`Failed to fetch alarms for device ${deviceId}:`, error);
      return [];
    }
  }
}

export default AlarmService;
