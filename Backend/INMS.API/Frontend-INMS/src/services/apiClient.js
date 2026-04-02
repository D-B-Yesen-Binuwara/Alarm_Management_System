import axios from 'axios';
import { environment } from '../config/environment';

const API_URL = environment.apiUrl;

const apiClient = axios.create({
  baseURL: API_URL,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
});

export default apiClient;
