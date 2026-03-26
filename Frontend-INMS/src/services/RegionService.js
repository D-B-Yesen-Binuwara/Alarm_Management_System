import apiClient from './apiClient';

/**
 * Region Service - handles all region-related API calls
 */
class RegionService {
  /**
   * Get all regions
   * @returns {Promise<Array>}
   */
  static async getAll() {
    try {
      const response = await apiClient.get('/region');
      return response.data;
    } catch (error) {
      console.error('Failed to fetch all regions:', error);
      throw error;
    }
  }

  /**
   * Get region by ID
   * @param {number} id
   * @returns {Promise<Object>}
   */
  static async getById(id) {
    try {
      const response = await apiClient.get(`/region/${id}`);
      return response.data;
    } catch (error) {
      console.error(`Failed to fetch region ${id}:`, error);
      throw error;
    }
  }
}

export default RegionService;
