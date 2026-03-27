import { Routes, Route, Navigate } from 'react-router-dom';
import PlaceholderPage from '../components/PlaceholderPage';
import Dashboard from '../pages/Dashboard';
import NetworkMap from '../pages/NetworkMap';
import ImpactAnalysis from '../pages/ImpactAnalysis';
import UserProfile from '../pages/UserProfile';

const AppRoutes = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="/dashboard" element={<Dashboard />} />
      <Route path="/network-map" element={<NetworkMap />} />
      <Route path="/impact-analysis" element={<ImpactAnalysis />} />
      <Route path="/profile" element={<UserProfile />} />

      <Route path="/events" element={<PlaceholderPage title="Events" />} />
      <Route path="/home" element={<PlaceholderPage title="Home" />} />
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
};

export default AppRoutes;
