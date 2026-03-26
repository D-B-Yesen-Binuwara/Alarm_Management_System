import { Link, useLocation } from 'react-router-dom';

const Navbar = () => {
  const location = useLocation();

  const isActive = (path) => location.pathname === path ? 'bg-green-800' : '';

  return (
    <nav className="bg-green-600 text-white px-6 py-3 flex items-center justify-between shadow-md">
      <span className="text-lg font-bold tracking-wide">INMS — NOC Operator Dashboard</span>
      
      <div className="flex gap-2">
        <Link 
          to="/dashboard"
          className={`px-4 py-1.5 rounded text-sm font-medium hover:bg-green-700 transition ${isActive('/dashboard')}`}
        >
          Dashboard
        </Link>
        <Link 
          to="/network-map"
          className={`px-4 py-1.5 rounded text-sm font-medium hover:bg-green-700 transition ${isActive('/network-map')}`}
        >
          Network Map
        </Link>
        <Link 
          to="/impact-analysis"
          className={`px-4 py-1.5 rounded text-sm font-medium hover:bg-green-700 transition ${isActive('/impact-analysis')}`}
        >
          Impact Analysis
        </Link>
        <Link 
          to="/events"
          className={`px-4 py-1.5 rounded text-sm font-medium hover:bg-green-700 transition ${isActive('/events')}`}
        >
          Correlation
        </Link>
        <Link 
          to="/home"
          className={`px-4 py-1.5 rounded text-sm font-medium hover:bg-green-700 transition ${isActive('/home')}`}
        >
          Alarm Logs
        </Link>
      </div>
    </nav>
  );
};

export default Navbar;
