import apiClient from './apiClient';

const VENDOR_PATHS = ['/vendors', '/vendor'];
const NODE_PATHS = ['/nodes', '/node'];
const CUSTOMER_PATHS = ['/customers', '/customer'];

async function requestWithFallback(method, paths, payload) {
  let lastError;

  for (const path of paths) {
    try {
      const response = payload === undefined
        ? await apiClient[method](path)
        : await apiClient[method](path, payload);
      return response.data;
    } catch (error) {
      lastError = error;
      const status = error?.response?.status;
      if (status != null && status !== 404) {
        throw error;
      }
    }
  }

  throw lastError ?? new Error('Request failed.');
}

async function requestByPathWithFallback(method, buildPathVariants, payload) {
  let lastError;

  for (const path of buildPathVariants()) {
    try {
      const response = payload === undefined
        ? await apiClient[method](path)
        : await apiClient[method](path, payload);
      return response.data;
    } catch (error) {
      lastError = error;
      const status = error?.response?.status;
      if (status != null && status !== 404) {
        throw error;
      }
    }
  }

  throw lastError ?? new Error('Request failed.');
}

class CorrelationService {
  static async getVendors() {
    return requestWithFallback('get', VENDOR_PATHS);
  }

  static async createVendor(vendor) {
    return requestWithFallback('post', VENDOR_PATHS, vendor);
  }

  static async updateVendor(id, vendor) {
    return requestByPathWithFallback('put', () => [
      `/vendors/${id}`,
      `/vendor/${id}`
    ], vendor);
  }

  static async deleteVendor(id) {
    return requestByPathWithFallback('delete', () => [
      `/vendors/${id}`,
      `/vendor/${id}`
    ]);
  }

  static async getNodes() {
    return requestWithFallback('get', NODE_PATHS);
  }

  static async assignNodeVendor(nodeId, vendorId) {
    return requestByPathWithFallback('put', () => [
      `/nodes/${nodeId}/assign-vendor`,
      `/node/${nodeId}/assign-vendor`
    ], { vendorId });
  }

  static async assignNodeCustomer(nodeId, customerId) {
    return requestByPathWithFallback('put', () => [
      `/nodes/${nodeId}/assign-customer`,
      `/node/${nodeId}/assign-customer`
    ], { customerId });
  }

  static async getCustomers() {
    return requestWithFallback('get', CUSTOMER_PATHS);
  }
}

export default CorrelationService;
