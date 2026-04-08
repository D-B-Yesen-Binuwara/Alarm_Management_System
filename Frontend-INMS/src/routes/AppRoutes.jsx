import { Routes, Route } from 'react-router-dom';
import UserManagement from '../pages/UserManagement';

const AppRoutes = () => (
  <Routes>
    <Route path="/user-management" element={<UserManagement />} />
  </Routes>
);

export default AppRoutes;
