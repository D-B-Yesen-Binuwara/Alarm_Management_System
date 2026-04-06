import { Link, useLocation } from 'react-router-dom';

const Navbar = () => {
  const location = useLocation();

  const isActive = (path) => location.pathname === path
    ? 'bg-gradient-to-r from-sky-500/25 to-emerald-400/25 text-white border border-sky-300/40 shadow-sm'
    : 'text-sky-100';

  return (
    <nav className="bg-gradient-to-r from-slate-900 via-sky-900 to-slate-800
    text-white px-6 py-3 flex items-center justify-between shadow-md border-b border-emerald-400/40">
      <div className="flex items-center gap-3 min-w-0">
        <img
          src="/sltmobitel-logo.png"
          alt="SLTMobitel"
          className="h-10 w-auto object-contain"
        />
        <span className="text-lg font-bold tracking-wide text-white truncate">
          Integrated Network Management System
        </span>
      </div>
      
      <div className="flex gap-2 flex-wrap justify-end">
        <Link 
          to="/dashboard"
          className={`px-4 py-1.5 rounded text-sm font-medium
            hover:bg-sky-400/20 hover:text-white transition ${isActive('/dashboard')}`}
        >
          Dashboard
        </Link>
        <Link 
          to="/network-map"
          className={`px-4 py-1.5 rounded text-sm font-medium
            hover:bg-emerald-400/20 hover:text-white transition ${isActive('/network-map')}`}
        >
          Network Map
        </Link>
        <Link 
          to="/impact-analysis"
          className={`px-4 py-1.5 rounded text-sm font-medium
            hover:bg-sky-400/20 hover:text-white transition ${isActive('/impact-analysis')}`}
        >
          Impact Analysis
        </Link>
        <Link 
          to="/events"
          className={`px-4 py-1.5 rounded text-sm font-medium
            hover:bg-emerald-400/20 hover:text-white transition ${isActive('/events')}`}
        >
          Correlation
        </Link>
        <Link 
          to="/home"
          className={`px-4 py-1.5 rounded text-sm font-medium
            hover:bg-sky-400/20 hover:text-white transition ${isActive('/home')}`}
        >
          Alarm Logs
        </Link>
        <div className="h-6 w-px bg-emerald-400/30 mx-1 align-middle self-center hidden md:block"></div>
        <Link 
          to="/profile"
          className={`flex items-center gap-2 px-4 py-1.5 rounded text-sm font-medium hover:bg-emerald-400/20 hover:text-white transition ${isActive('/profile')}`}
        >
          <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"></path>
          </svg>
          Profile
        </Link>
      </div>
    </nav>
  );
};

export default Navbar;
