import { useEffect, useMemo, useState } from 'react'

const initialUsers = [
  { id: 1, username: 'jdoe', fullName: 'Jane Doe', email: 'jane.doe@example.com', role: 'Network Engineer', status: 'Active', lastLogin: '2024-01-15' },
  { id: 2, username: 'mthompson', fullName: 'Mike Thompson', email: 'mike.t@example.com', role: 'Support', status: 'Active', lastLogin: '2024-01-14' },
  { id: 3, username: 'snguyen', fullName: 'Sarah Nguyen', email: 'sarah.n@example.com', role: 'Admin', status: 'Inactive', lastLogin: '2024-01-10' },
  { id: 4, username: 'rpatel', fullName: 'Raj Patel', email: 'raj.p@example.com', role: 'Developer', status: 'Active', lastLogin: '2024-01-15' },
  { id: 5, username: 'lchen', fullName: 'Lisa Chen', email: 'lisa.c@example.com', role: 'Analyst', status: 'Active', lastLogin: '2024-01-13' },
]

const emptyForm = { username: '', fullName: '', email: '', role: '', status: 'Active' }

export default function UserManagement() {
  const [users, setUsers] = useState(() => {
    try {
      const saved = localStorage.getItem('inms-users')
      return saved ? JSON.parse(saved) : initialUsers
    } catch {
      return initialUsers
    }
  })

  const [showModal, setShowModal] = useState(false)
  const [modalMode, setModalMode] = useState('add') // add, edit
  const [form, setForm] = useState(emptyForm)
  const [editingUser, setEditingUser] = useState(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState('All')
  const [roleFilter, setRoleFilter] = useState('All')
  const [message, setMessage] = useState('')

  useEffect(() => {
    localStorage.setItem('inms-users', JSON.stringify(users))
  }, [users])

  const filteredUsers = useMemo(() => {
    return users.filter(user => {
      const matchesSearch = user.fullName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                           user.username.toLowerCase().includes(searchTerm.toLowerCase()) ||
                           user.email.toLowerCase().includes(searchTerm.toLowerCase())
      const matchesStatus = statusFilter === 'All' || user.status === statusFilter
      const matchesRole = roleFilter === 'All' || user.role === roleFilter
      return matchesSearch && matchesStatus && matchesRole
    })
  }, [users, searchTerm, statusFilter, roleFilter])

  const stats = useMemo(() => {
    const total = users.length
    const active = users.filter(u => u.status === 'Active').length
    const inactive = users.filter(u => u.status === 'Inactive').length
    const roles = [...new Set(users.map(u => u.role))].length
    return { total, active, inactive, roles }
  }, [users])

  const isValid = useMemo(() => {
    return form.username.trim() && form.fullName.trim() && form.email.trim() && form.role.trim()
  }, [form])

  const clearFeedback = () => setMessage('')

  const handleInputChange = (field, value) => {
    setForm((prev) => ({ ...prev, [field]: value }))
    clearFeedback()
  }

  const resetForm = () => {
    setForm(emptyForm)
    setEditingUser(null)
  }

  const openModal = (mode, user = null) => {
    setModalMode(mode)
    if (mode === 'edit' && user) {
      setForm({
        username: user.username,
        fullName: user.fullName,
        email: user.email,
        role: user.role,
        status: user.status
      })
      setEditingUser(user)
    } else {
      resetForm()
    }
    setShowModal(true)
    clearFeedback()
  }

  const closeModal = () => {
    setShowModal(false)
    resetForm()
  }

  const handleSubmit = () => {
    if (!isValid) {
      setMessage('Please complete all required fields.')
      return
    }

    if (modalMode === 'add') {
      const nextId = users.length ? Math.max(...users.map((u) => u.id)) + 1 : 1
      setUsers((prev) => [...prev, {
        id: nextId,
        ...form,
        lastLogin: new Date().toISOString().split('T')[0]
      }])
      setMessage('User created successfully!')
    } else {
      setUsers((prev) => prev.map((user) =>
        user.id === editingUser.id ? { ...user, ...form } : user
      ))
      setMessage('User updated successfully!')
    }

    closeModal()
    setTimeout(() => setMessage(''), 3000)
  }

  const handleDelete = (userId) => {
    if (!window.confirm('Are you sure you want to delete this user? This action cannot be undone.')) return
    setUsers((prev) => prev.filter((user) => user.id !== userId))
    setMessage('User deleted successfully!')
    setTimeout(() => setMessage(''), 3000)
  }

  const getStatusColor = (status) => {
    return status === 'Active' 
      ? { backgroundColor: '#eff6ff', color: '#1d4ed8', border: '1px solid #bfdbfe' }
      : { backgroundColor: '#f8fafc', color: '#64748b', border: '1px solid #e2e8f0' }
  }

  const getRoleColor = (role) => {
    // Monochromatic sleek approach instead of rainbow
    return { backgroundColor: '#f1f5f9', color: '#334155', border: '1px solid #cbd5e1' }
  }

  return (
    <div className="um-dashboard">
      {/* Header */}
      <header className="um-header">
        <div className="um-header-content">
          <div>
            <h1>User Management Dashboard</h1>
            <p>Manage system users, roles, and permissions</p>
          </div>
          <button className="um-add-btn" onClick={() => openModal('add')}>
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
            Add New User
          </button>
        </div>
      </header>

      {/* Stats Cards */}
      <div className="um-stats">
        <div className="um-stat-card">
          <div className="um-stat-header">
            <div className="um-stat-icon total">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
            </div>
            <div className="um-stat-title">Total Users</div>
          </div>
          <div className="um-stat-content">
            <h3>{stats.total}</h3>
          </div>
        </div>
        <div className="um-stat-card">
          <div className="um-stat-header">
            <div className="um-stat-icon active">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><polyline points="16 11 18 13 22 9"/></svg>
            </div>
            <div className="um-stat-title">Active Users</div>
          </div>
          <div className="um-stat-content">
            <h3>{stats.active}</h3>
          </div>
        </div>
        <div className="um-stat-card">
          <div className="um-stat-header">
            <div className="um-stat-icon inactive">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><line x1="17" y1="8" x2="23" y2="14"/><line x1="23" y1="8" x2="17" y2="14"/></svg>
            </div>
            <div className="um-stat-title">Inactive Users</div>
          </div>
          <div className="um-stat-content">
            <h3>{stats.inactive}</h3>
          </div>
        </div>
        <div className="um-stat-card">
          <div className="um-stat-header">
            <div className="um-stat-icon roles">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
            </div>
            <div className="um-stat-title">Unique Roles</div>
          </div>
          <div className="um-stat-content">
            <h3>{stats.roles}</h3>
          </div>
        </div>
      </div>

      {/* Filters and Search */}
      <div className="um-filters">
        <div className="um-search">
          <svg className="um-search-icon" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
          <input
            type="text"
            placeholder="Search users..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        <div className="um-filter-group">
          <select value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
            <option value="All">All Status</option>
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
          </select>
          <select value={roleFilter} onChange={(e) => setRoleFilter(e.target.value)}>
            <option value="All">All Roles</option>
            <option value="Admin">Admin</option>
            <option value="Network Engineer">Network Engineer</option>
            <option value="Developer">Developer</option>
            <option value="Support">Support</option>
            <option value="Analyst">Analyst</option>
          </select>
        </div>
      </div>

      {/* Message */}
      {message && (
        <div className={`um-message ${message.includes('deleted') ? 'error' : 'success'}`}>
          {message}
        </div>
      )}

      {/* Users Table */}
      <div className="um-table-container">
        <table className="um-table">
          <thead>
            <tr>
              <th>User</th>
              <th>Role</th>
              <th>Status</th>
              <th>Last Login</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {filteredUsers.length === 0 ? (
              <tr>
                <td colSpan="5" className="um-empty">
                  <div className="um-empty-icon">
                    <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"><circle cx="12" cy="12" r="10"/><path d="M16 16s-1.5-2-4-2-4 2-4 2"/><line x1="9" y1="9" x2="9.01" y2="9"/><line x1="15" y1="9" x2="15.01" y2="9"/></svg>
                  </div>
                  {searchTerm || statusFilter !== 'All' || roleFilter !== 'All'
                    ? 'No users match your filters.'
                    : 'No users found. Add your first user to get started.'}
                </td>
              </tr>
            ) : (
              filteredUsers.map((user) => (
                <tr key={user.id}>
                  <td className="um-user-cell">
                    <div className="um-user-info">
                      <div className="um-avatar">{user.fullName.charAt(0).toUpperCase()}</div>
                      <div className="um-user-text">
                        <div className="um-user-name">{user.fullName}</div>
                        <div className="um-user-details">{user.username} • {user.email}</div>
                      </div>
                    </div>
                  </td>
                  <td>
                    <span className="um-role-badge" style={getRoleColor(user.role)}>
                      {user.role}
                    </span>
                  </td>
                  <td>
                    <span className="um-status-badge" style={getStatusColor(user.status)}>
                      {user.status}
                    </span>
                  </td>
                  <td>{user.lastLogin}</td>
                  <td>
                    <div className="um-actions">
                      <button className="um-icon-btn" onClick={() => openModal('edit', user)} title="Edit user">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                      </button>
                      <button className="um-icon-btn delete" onClick={() => handleDelete(user.id)} title="Delete user">
                        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><path d="M3 6h18"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/><line x1="10" y1="11" x2="10" y2="17"/><line x1="14" y1="11" x2="14" y2="17"/></svg>
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Modal */}
      {showModal && (
        <div className="um-modal-overlay" onClick={closeModal}>
          <div className="um-modal" onClick={(e) => e.stopPropagation()}>
            <div className="um-modal-header">
              <h2>{modalMode === 'add' ? 'Add New User' : 'Edit User'}</h2>
              <button className="um-modal-close" onClick={closeModal}>
                <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>
            <div className="um-modal-body">
              <form onSubmit={(e) => { e.preventDefault(); handleSubmit(); }}>
                <div className="um-form-row">
                  <div className="um-form-group">
                    <label>Username *</label>
                    <input
                      type="text"
                      value={form.username}
                      onChange={(e) => handleInputChange('username', e.target.value)}
                      placeholder="Enter username"
                      required
                    />
                  </div>
                  <div className="um-form-group">
                    <label>Full Name *</label>
                    <input
                      type="text"
                      value={form.fullName}
                      onChange={(e) => handleInputChange('fullName', e.target.value)}
                      placeholder="Enter full name"
                      required
                    />
                  </div>
                </div>
                <div className="um-form-group">
                  <label>Email *</label>
                  <input
                    type="email"
                    value={form.email}
                    onChange={(e) => handleInputChange('email', e.target.value)}
                    placeholder="Enter email address"
                    required
                  />
                </div>
                <div className="um-form-row">
                  <div className="um-form-group">
                    <label>Role *</label>
                    <select value={form.role} onChange={(e) => handleInputChange('role', e.target.value)} required>
                      <option value="">Select role</option>
                      <option value="Admin">Admin</option>
                      <option value="Network Engineer">Network Engineer</option>
                      <option value="Developer">Developer</option>
                      <option value="Support">Support</option>
                      <option value="Analyst">Analyst</option>
                    </select>
                  </div>
                  <div className="um-form-group">
                    <label>Status</label>
                    <select value={form.status} onChange={(e) => handleInputChange('status', e.target.value)}>
                      <option value="Active">Active</option>
                      <option value="Inactive">Inactive</option>
                    </select>
                  </div>
                </div>
              </form>
            </div>
            <div className="um-modal-footer">
              <button className="um-cancel-btn" onClick={closeModal}>Cancel</button>
              <button className="um-submit-btn" onClick={handleSubmit} disabled={!isValid}>
                {modalMode === 'add' ? 'Create User' : 'Update User'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
