import React from 'react'

export default function Layout({ children }) {
  return (
    <div className="layout-wrapper">
      {/* Fixed Sidebar */}
      <aside className="app-sidebar">
        <div className="sidebar-brand">
          <img src="/logo.png" alt="SLTMOBITEL Logo" className="sidebar-logo" style={{ height: '32px', objectFit: 'contain' }} />
        </div>
        
        <div className="sidebar-scrollable">
          <nav className="sidebar-nav">
            <a href="#" className="sidebar-link active">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="m3 9 9-7 9 7v11a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z"/><polyline points="9 22 9 12 15 12 15 22"/></svg>
              Dashboard
            </a>
            <a href="#" className="sidebar-link">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><rect width="8" height="6" x="8" y="2" rx="1"/><rect width="8" height="6" x="2" y="14" rx="1"/><rect width="8" height="6" x="14" y="14" rx="1"/><path d="M12 8v2"/><path d="M6 14v-2c0-1 1-2 2-2h8c1 0 2 1 2 2v2"/></svg>
              Network Map
            </a>
            <a href="#" className="sidebar-link">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/></svg>
              Impact Analysis
            </a>
            <a href="#" className="sidebar-link">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M10 13a5 5 0 0 0 7.54.54l3-3a5 5 0 0 0-7.07-7.07l-1.72 1.71"/><path d="M14 11a5 5 0 0 0-7.54-.54l-3 3a5 5 0 0 0 7.07 7.07l1.71-1.71"/></svg>
              Correlation
            </a>
            <a href="#" className="sidebar-link">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M6 8a6 6 0 0 1 12 0c0 7 3 9 3 9H3s3-2 3-9"/><path d="M10.3 21a1.94 1.94 0 0 0 3.4 0"/></svg>
              Alarm Logs
            </a>
          </nav>
        </div>

        <div className="sidebar-footer-links">
          <a href="#" className="sidebar-link sub-link">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" x2="9" y1="12" y2="12"/></svg>
            Logout
          </a>
          <span className="sidebar-version">v1.0.0</span>
        </div>
      </aside>

      {/* Main Layout Area */}
      <div className="app-main-panel">
        <header className="top-header">
          <h2 className="page-title">Dashboard</h2>
          
          <div className="header-profile">
            <div className="profile-text">
              <span className="welcome-text">Welcome back,</span>
              <span className="user-name">G.V.R.Ruksala</span>
            </div>
            <div className="header-avatar">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
            </div>
          </div>
        </header>

        <main className="content-scrollable">
          {children}
        </main>
      </div>
    </div>
  )
}
