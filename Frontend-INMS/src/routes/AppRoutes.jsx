import { Routes, Route, Navigate } from 'react-router-dom';
import PlaceholderPage from '../components/PlaceholderPage';
import Dashboard from '../pages/Dashboard';
import NetworkMap from '../pages/NetworkMap';
import ImpactAnalysis from '../pages/ImpactAnalysis';
import Login from '../pages/Login';
import UserRegistration from '../pages/UserRegistration';
// import ProtectedRoute from './ProtectedRoute';

const AppRoutes = () => {
  return (
    <Routes>
      <Route path="/" element={<Navigate to="/dashboard" replace />} />
      <Route path="/login" element={<Login />} />
      <Route path="/register" element={<UserRegistration />} />
      {/*
      <Route
        path="/dashboard"
        element={(
          <ProtectedRoute>
            <Dashboard />
          </ProtectedRoute>
        )}
      />
      <Route
        path="/network-map"
        element={(
          <ProtectedRoute>
            <NetworkMap />
          </ProtectedRoute>
        )}
      />
      <Route
        path="/impact-analysis"
        element={(
          <ProtectedRoute>
            <ImpactAnalysis />
          </ProtectedRoute>
        )}
      />
      */}
      <Route path="/dashboard" element={<Dashboard />} />
      <Route path="/network-map" element={<NetworkMap />} />
      <Route path="/impact-analysis" element={<ImpactAnalysis />} />

      {/*
      <Route
        path="/events"
        element={(
          <ProtectedRoute>
            <PlaceholderPage title="Events" />
          </ProtectedRoute>
        )}
      />
      <Route
        path="/home"
        element={(
          <ProtectedRoute>
            <PlaceholderPage title="Home" />
          </ProtectedRoute>
        )}
      />
      */}
      <Route path="/correlation" element={<PlaceholderPage title="Correlation" />} />
      <Route path="/alarmlogs" element={<PlaceholderPage title="AlarmLogs" />} />
      <Route path="*" element={<Navigate to="/dashboard" replace />} />
    </Routes>
  );
};

export default AppRoutes;
