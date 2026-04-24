import apiClient from './apiClient';

/**
 * User Service - handles user-related API calls
 */
class UserService {
  /**
   * Get all users
   * @returns {Promise<Array>}
   */
  static async getAll() {
    try {
      const response = await apiClient.get('/user');
      return response.data;
    } catch (error) {
      console.error('Failed to fetch users:', error);
      throw error;
    }
  }
}

export default UserService;
