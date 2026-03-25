import { useState, useEffect, useRef } from 'react';
import L from 'leaflet';
import 'leaflet/dist/leaflet.css';
import DeviceService from '../services/DeviceService';
import { normalizeStatus } from '../utils/formatters';

const NetworkMap = () => {
  const mapContainer = useRef(null);
  const mapInstance = useRef(null);
  const markerLayer = useRef(L.layerGroup());

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  const MARKER_SHADOW_URL = 'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.9.4/images/marker-shadow.png';
  const GREEN_MARKER_URL = 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-green.png';
  const RED_MARKER_URL = 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-red.png';
  const YELLOW_MARKER_URL = 'https://raw.githubusercontent.com/pointhi/leaflet-color-markers/master/img/marker-icon-2x-yellow.png';

  const initializeMap = () => {
    if (mapInstance.current) {
      return;
    }

    mapInstance.current = L.map(mapContainer.current).setView([7.8731, 80.7718], 7);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(mapInstance.current);

    markerLayer.current.addTo(mapInstance.current);
    
    setTimeout(() => {
      if (mapInstance.current) {
        mapInstance.current.invalidateSize();
      }
    }, 0);
  };

  const getMarkerIcon = (status, isImpacted) => {
    let iconUrl;

    if (normalizeStatus(status) === 'DOWN') {
      iconUrl = RED_MARKER_URL;
    } else if (isImpacted === 1) {
      iconUrl = YELLOW_MARKER_URL;
    } else {
      iconUrl = GREEN_MARKER_URL;
    }

    return L.icon({
      iconUrl,
      shadowUrl: MARKER_SHADOW_URL,
      iconSize: [25, 41],
      iconAnchor: [12, 41],
      popupAnchor: [1, -34],
      shadowSize: [41, 41]
    });
  };

  const plotMarkers = (devices) => {
    markerLayer.current.clearLayers();

    for (const device of devices) {
      L.marker([device.latitude, device.longitude], {
        icon: getMarkerIcon(device.status, device.isImpacted)
      })
        .bindPopup(
          `<strong>Device ID:</strong> ${device.deviceId}<br>
           <strong>Device Name:</strong> ${device.deviceName}<br>
           <strong>Device Type:</strong> ${device.deviceType}<br>
           <strong>Status:</strong> ${normalizeStatus(device.status)}`
        )
        .addTo(markerLayer.current);
    }
  };

  const loadMapDevices = async () => {
    try {
      const devices = await DeviceService.getDevicesForMap();
      plotMarkers(devices);
      if (mapInstance.current) {
        mapInstance.current.invalidateSize();
      }
      setLoading(false);
    } catch (err) {
      setError('Failed to load map devices. Is the API running?');
      setLoading(false);
      console.error(err);
    }
  };

  useEffect(() => {
    setLoading(true);
    initializeMap();
    loadMapDevices();

    // Cleanup
    return () => {
      if (mapInstance.current) {
        mapInstance.current.remove();
        mapInstance.current = null;
      }
    };
  }, []);

  return (
    <div className="min-h-screen bg-gray-100 py-8 px-4">
      <div className="mx-auto pr-4 pl-4">
        <div className="bg-white rounded-lg border-2 border-gray-300 shadow-lg overflow-hidden">
          {/* Header */}
          <div className="px-6 py-4 border-b border-gray-300 bg-gray-50">
            <h2 className="text-xl font-bold text-gray-800">Network Map - Sri Lanka</h2>
            <p className="text-sm text-gray-600 mt-1">Real-time network device visualization and monitoring</p>
          </div>

          {/* Error */}
          {error && (
            <div className="m-6 bg-red-50 border-2 border-red-300 text-red-700 rounded-lg px-6 py-4">
              <div className="flex items-start">
                <span className="text-2xl mr-3">⚠️</span>
                <div>
                  <h3 className="font-semibold">Failed to Load Map</h3>
                  <p className="text-sm mt-1">{error}</p>
                </div>
              </div>
            </div>
          )}

          {/* Map Container */}
          <div className="border-t border-gray-300 relative">
            <div 
              ref={mapContainer}
              style={{ height: '600px' }}
              className="w-full"
            ></div>

            {loading && (
              <div className="absolute inset-0 flex items-center justify-center text-gray-500 bg-white/75">
                <svg className="animate-spin h-8 w-8 mr-3 text-green-600" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z"></path>
                </svg>
                <span className="text-lg font-medium">Loading map...</span>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default NetworkMap;
