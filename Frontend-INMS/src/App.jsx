import { useState } from 'react';
import { BrowserRouter as Router } from 'react-router-dom';
import Navbar from './components/Navbar';
import Sidebar from './components/Sidebar';
import AppRoutes from './routes/AppRoutes';
// import './App.css';

function App() {
  const [collapsed, setCollapsed] = useState(false);

  return (
    <Router>
      <div className="min-h-screen flex flex-col bg-gray-50 text-gray-900">
        <Navbar onToggle={() => setCollapsed(p => !p)} />
        <div className="flex flex-1 overflow-hidden">
          <Sidebar collapsed={collapsed} />
          <main className="flex-1 overflow-y-auto p-6">
            <AppRoutes />
          </main>
        </div>
      </div>
    </Router>
  );
}

export default App;
