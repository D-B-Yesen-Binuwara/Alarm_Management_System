import { Routes, Route, Navigate } from 'react-router-dom';
import PlaceholderPage from '../components/PlaceholderPage';
import Dashboard from '../pages/Dashboard';
import NetworkMap from '../pages/NetworkMap';
import ImpactAnalysis from '../pages/ImpactAnalysis';
import Login from '../pages/Login';
import Register from '../pages/Register';
import EventPage from '../pages/EventPage';

// ✅ TEMP CORRELATION PAGE
const CorrelationPage = () => {
    return <div className="p-6 text-xl">Correlation Page (Coming Soon)</div>;
};

const AppRoutes = () => {
    return (
        <Routes>

            <Route path="/" element={<Navigate to="/login" replace />} />

            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />

            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/network-map" element={<NetworkMap />} />
            <Route path="/impact-analysis" element={<ImpactAnalysis />} />

            {/* ✅ FIXED */}
            <Route path="/correlation" element={<CorrelationPage />} />

            {/* ✅ YOUR EVENT PAGE */}
            <Route path="/events" element={<EventPage />} />

            <Route path="/home" element={<PlaceholderPage title="Home" />} />

            <Route path="*" element={<Navigate to="/login" replace />} />

        </Routes>
    );
};

export default AppRoutes;