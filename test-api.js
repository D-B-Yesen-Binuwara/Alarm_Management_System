// Simple test script to verify API endpoints
const axios = require('axios');

const API_BASE = 'http://localhost:5289/api';

async function testAPI() {
  console.log('Testing API endpoints...\n');

  try {
    // Test 1: Get all devices
    console.log('1. Testing GET /api/device');
    const devicesResponse = await axios.get(`${API_BASE}/device`);
    console.log(`✓ Found ${devicesResponse.data.length} devices`);
    
    if (devicesResponse.data.length > 0) {
      const firstDevice = devicesResponse.data[0];
      console.log(`   First device: ${firstDevice.deviceName} (ID: ${firstDevice.deviceId})`);
      
      // Test 2: Get specific device
      console.log(`\n2. Testing GET /api/device/${firstDevice.deviceId}`);
      const deviceResponse = await axios.get(`${API_BASE}/device/${firstDevice.deviceId}`);
      console.log(`✓ Device details: ${deviceResponse.data.deviceName}`);
      
      // Test 3: Get impact result (should work even if no analysis done yet)
      console.log(`\n3. Testing GET /api/impact-analysis/result/${firstDevice.deviceId}`);
      const impactResponse = await axios.get(`${API_BASE}/impact-analysis/result/${firstDevice.deviceId}`);
      console.log(`✓ Impact result: Device ${impactResponse.data.Device?.DeviceName}, Impacted devices: ${impactResponse.data.ImpactedDevices?.length || 0}`);
      
      // Test 4: Run analysis
      console.log(`\n4. Testing POST /api/impact-analysis/analyze/${firstDevice.deviceId}`);
      const analysisResponse = await axios.post(`${API_BASE}/impact-analysis/analyze/${firstDevice.deviceId}`);
      console.log(`✓ Analysis completed: Device ${analysisResponse.data.Device?.DeviceName}, Impacted devices: ${analysisResponse.data.ImpactedDevices?.length || 0}`);
      
      // Test 5: Clear analysis
      console.log(`\n5. Testing POST /api/impact-analysis/clear/${firstDevice.deviceId}`);
      const clearResponse = await axios.post(`${API_BASE}/impact-analysis/clear/${firstDevice.deviceId}`);
      console.log(`✓ Analysis cleared: Device ${clearResponse.data.Device?.DeviceName}`);
    }
    
    console.log('\n✓ All API tests passed!');
    
  } catch (error) {
    console.error('✗ API test failed:', error.message);
    if (error.response) {
      console.error('Response status:', error.response.status);
      console.error('Response data:', error.response.data);
    }
  }
}

testAPI();