import apiClient from './apiClient';

/**
 * ImpactAnalysisService - Handles all impact analysis API operations
 * Communicates with backend endpoints for failure analysis and impact propagation
 */
class ImpactAnalysisService {
  /**
   * Get impact analysis result for a specific device (root cause + impacted devices)
   * GET /api/impact-analysis/result/{deviceId}
   */
  static async getImpactResult(deviceId) {
    try {
      console.log(`Fetching impact result for device ${deviceId}`);
      const response = await apiClient.get(`/impact-analysis/result/${deviceId}`);
      console.log(`Impact result response:`, response.data);
      return response.data;
    } catch (error) {
      console.error(`Failed to fetch impact analysis for device ${deviceId}:`, error);
      if (error.response) {
        console.error('Response data:', error.response.data);
        console.error('Response status:', error.response.status);
      }
      throw error;
    }
  }

  /**
   * Trigger failure analysis for a device
   * Marks device as DOWN and analyzes downstream impact
   * POST /api/impact-analysis/analyze/{deviceId}
   */
  static async analyzeDeviceFailure(deviceId) {
    try {
      console.log(`Starting failure analysis for device ${deviceId}`);
      const response = await apiClient.post(`/impact-analysis/analyze/${deviceId}`);
      console.log(`Analysis response:`, response.data);
      return response.data;
    } catch (error) {
      console.error(`Failed to analyze failure for device ${deviceId}:`, error);
      if (error.response) {
        console.error('Response data:', error.response.data);
        console.error('Response status:', error.response.status);
      }
      throw error;
    }
  }

  /**
   * Clear impact analysis for a device (device recovery)
   * Marks device as UP and clears all related alarms and impact records
   * POST /api/impact-analysis/clear/{deviceId}
   */
  static async clearDeviceImpact(deviceId) {
    try {
      const response = await apiClient.post(`/impact-analysis/clear/${deviceId}`);
      return response.data;
    } catch (error) {
      console.error(`Failed to clear impact for device ${deviceId}:`, error);
      throw error;
    }
  }
}

export default ImpactAnalysisService;
