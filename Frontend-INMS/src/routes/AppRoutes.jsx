import { Routes, Route, Navigate } from 'react-router-dom';
import PlaceholderPage from '../components/PlaceholderPage';
import Dashboard from '../pages/Dashboard';
import NetworkMap from '../pages/NetworkMap';
import ImpactAnalysis from '../pages/ImpactAnalysis';
import UserProfile from '../pages/UserProfile';
import UserManagement from '../pages/UserManagement';
import DeviceManagement from '../pages/DeviceManagement';
import Register from '../pages/Register';
import Login from '../pages/Login';

const AppRoutes = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="/dashboard" element={<Dashboard />} />
      <Route path="/network-map" element={<NetworkMap />} />
      <Route path="/impact-analysis" element={<ImpactAnalysis />} />
      <Route path="/device-management" element={<DeviceManagement />} />
      <Route path="/profile" element={<UserProfile />} />
      <Route path="/user-management" element={<UserManagement />} />
      <Route path="/events" element={<PlaceholderPage title="Events" />} />
      <Route path="/home" element={<PlaceholderPage title="Home" />} />
      <Route path="*" element={<Navigate to="/dashboard" replace />} />

      {/* <Route path="/" element={<Navigate to="/login" replace />} /> */}
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<Register />} />
    </Routes>
  );
};

export default AppRoutes;
