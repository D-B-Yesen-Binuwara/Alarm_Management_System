import { useEffect, useMemo, useState } from 'react'

const initialUsers = [
  { id: 1, username: 'jdoe', fullName: 'Jane Doe', email: 'jane.doe@example.com', role: 'Network Engineer', status: 'Active', lastLogin: '2024-01-15' },
  { id: 2, username: 'mthompson', fullName: 'Mike Thompson', email: 'mike.t@example.com', role: 'Support', status: 'Active', lastLogin: '2024-01-14' },
  { id: 3, username: 'snguyen', fullName: 'Sarah Nguyen', email: 'sarah.n@example.com', role: 'Admin', status: 'Inactive', lastLogin: '2024-01-10' },
  { id: 4, username: 'rpatel', fullName: 'Raj Patel', email: 'raj.p@example.com', role: 'Developer', status: 'Active', lastLogin: '2024-01-15' },
  { id: 5, username: 'lchen', fullName: 'Lisa Chen', email: 'lisa.c@example.com', role: 'Analyst', status: 'Active', lastLogin: '2024-01-13' },
]

const emptyForm = { username: '', fullName: '', email: '', role: '', status: 'Active' }

const inputCls = 'w-full px-3 py-2 border border-slate-300 rounded-lg text-sm text-slate-900 bg-white focus:outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/10'
const selectCls = inputCls + ' appearance-none'

export default function UserManagement() {
  const [users, setUsers] = useState(() => {
    try { const s = localStorage.getItem('inms-users'); return s ? JSON.parse(s) : initialUsers } catch { return initialUsers }
  })
  const [showModal, setShowModal] = useState(false)
  const [modalMode, setModalMode] = useState('add')
  const [form, setForm] = useState(emptyForm)
  const [editingUser, setEditingUser] = useState(null)
  const [searchTerm, setSearchTerm] = useState('')
  const [statusFilter, setStatusFilter] = useState('All')
  const [roleFilter, setRoleFilter] = useState('All')
  const [message, setMessage] = useState('')

  useEffect(() => { localStorage.setItem('inms-users', JSON.stringify(users)) }, [users])

  const filteredUsers = useMemo(() => users.filter(u => {
    const s = searchTerm.toLowerCase()
    return (u.fullName.toLowerCase().includes(s) || u.username.toLowerCase().includes(s) || u.email.toLowerCase().includes(s))
      && (statusFilter === 'All' || u.status === statusFilter)
      && (roleFilter === 'All' || u.role === roleFilter)
  }), [users, searchTerm, statusFilter, roleFilter])

  const stats = useMemo(() => ({
    total: users.length,
    active: users.filter(u => u.status === 'Active').length,
    inactive: users.filter(u => u.status === 'Inactive').length,
    roles: [...new Set(users.map(u => u.role))].length,
  }), [users])

  const isValid = useMemo(() => form.username.trim() && form.fullName.trim() && form.email.trim() && form.role.trim(), [form])

  const handleInputChange = (field, value) => { setForm(p => ({ ...p, [field]: value })); setMessage('') }

  const openModal = (mode, user = null) => {
    setModalMode(mode)
    if (mode === 'edit' && user) { setForm({ username: user.username, fullName: user.fullName, email: user.email, role: user.role, status: user.status }); setEditingUser(user) }
    else { setForm(emptyForm); setEditingUser(null) }
    setShowModal(true); setMessage('')
  }

  const closeModal = () => { setShowModal(false); setForm(emptyForm); setEditingUser(null) }

  const handleSubmit = () => {
    if (!isValid) { setMessage('Please complete all required fields.'); return }
    if (modalMode === 'add') {
      const nextId = users.length ? Math.max(...users.map(u => u.id)) + 1 : 1
      setUsers(p => [...p, { id: nextId, ...form, lastLogin: new Date().toISOString().split('T')[0] }])
      setMessage('User created successfully!')
    } else {
      setUsers(p => p.map(u => u.id === editingUser.id ? { ...u, ...form } : u))
      setMessage('User updated successfully!')
    }
    closeModal(); setTimeout(() => setMessage(''), 3000)
  }

  const handleDelete = (id) => {
    if (!window.confirm('Delete this user? This cannot be undone.')) return
    setUsers(p => p.filter(u => u.id !== id))
    setMessage('User deleted successfully!'); setTimeout(() => setMessage(''), 3000)
  }

  const statusStyle = s => s === 'Active'
    ? 'bg-blue-50 text-blue-700 border border-blue-200'
    : 'bg-slate-100 text-slate-500 border border-slate-200'

  const statCards = [
    { label: 'Total Users', value: stats.total, icon: <><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M22 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></> },
    { label: 'Active Users', value: stats.active, icon: <><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><polyline points="16 11 18 13 22 9"/></> },
    { label: 'Inactive Users', value: stats.inactive, icon: <><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><line x1="17" y1="8" x2="23" y2="14"/><line x1="23" y1="8" x2="17" y2="14"/></> },
    { label: 'Unique Roles', value: stats.roles, icon: <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/> },
  ]

  return (
    <div className="max-w-7xl mx-auto flex flex-col gap-6">
      {/* Header */}
      <header className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-semibold text-slate-900 tracking-tight">User Management Dashboard</h1>
          <p className="text-sm text-slate-500 mt-1">Manage system users, roles, and permissions</p>
        </div>
        <button onClick={() => openModal('add')}
          className="flex items-center gap-2 px-4 py-2 rounded-lg text-sm font-medium text-white bg-gradient-to-r from-indigo-600 to-blue-500 hover:from-indigo-700 hover:to-blue-600 shadow-md transition">
          <svg className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
          Add New User
        </button>
      </header>

      {/* Stats */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        {statCards.map(({ label, value, icon }) => (
          <div key={label} className="bg-white rounded-xl border border-slate-200 p-5 shadow-sm hover:-translate-y-0.5 transition">
            <div className="flex items-center gap-3 mb-3">
              <div className="w-9 h-9 rounded-lg bg-blue-50 text-blue-600 flex items-center justify-center">
                <svg className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24">{icon}</svg>
              </div>
              <span className="text-sm text-slate-500 font-medium">{label}</span>
            </div>
            <p className="text-3xl font-bold text-slate-900">{value}</p>
          </div>
        ))}
      </div>

      {/* Filters */}
      <div className="bg-white rounded-xl border border-slate-200 p-4 shadow-sm flex flex-wrap gap-3 items-center justify-between">
        <div className="relative flex-1 min-w-48 max-w-xs">
          <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-400" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
          <input type="text" placeholder="Search users..." value={searchTerm} onChange={e => setSearchTerm(e.target.value)}
            className="w-full pl-9 pr-3 py-2 border border-slate-300 rounded-lg text-sm bg-slate-50 focus:outline-none focus:border-indigo-500 focus:bg-white focus:ring-2 focus:ring-indigo-500/10" />
        </div>
        <div className="flex gap-3">
          <select value={statusFilter} onChange={e => setStatusFilter(e.target.value)} className={selectCls + ' min-w-32'}>
            <option value="All">All Status</option>
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
          </select>
          <select value={roleFilter} onChange={e => setRoleFilter(e.target.value)} className={selectCls + ' min-w-36'}>
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
        <div className={`px-4 py-3 rounded-lg text-sm font-medium flex items-center gap-2 ${message.includes('deleted') ? 'bg-red-50 text-red-800 border border-red-200' : 'bg-emerald-50 text-emerald-800 border border-emerald-200'}`}>
          {message}
        </div>
      )}

      {/* Table */}
      <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
        <table className="w-full border-collapse">
          <thead className="bg-slate-50 border-b border-slate-200">
            <tr>
              {['User', 'Role', 'Status', 'Last Login', 'Actions'].map(h => (
                <th key={h} className="px-6 py-3 text-left text-xs font-medium text-slate-500 uppercase tracking-wider">{h}</th>
              ))}
            </tr>
          </thead>
          <tbody>
            {filteredUsers.length === 0 ? (
              <tr><td colSpan="5" className="text-center py-16 text-slate-400">
                <svg className="w-12 h-12 mx-auto mb-3 text-slate-300" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10"/><path d="M16 16s-1.5-2-4-2-4 2-4 2"/><line x1="9" y1="9" x2="9.01" y2="9"/><line x1="15" y1="9" x2="15.01" y2="9"/></svg>
                {searchTerm || statusFilter !== 'All' || roleFilter !== 'All' ? 'No users match your filters.' : 'No users found.'}
              </td></tr>
            ) : filteredUsers.map(user => (
              <tr key={user.id} className="border-b border-slate-100 hover:bg-slate-50 transition">
                <td className="px-6 py-4">
                  <div className="flex items-center gap-3">
                    <div className="w-9 h-9 rounded-full bg-slate-100 border border-slate-200 text-indigo-600 flex items-center justify-center font-semibold text-sm">
                      {user.fullName.charAt(0).toUpperCase()}
                    </div>
                    <div>
                      <div className="text-sm font-medium text-slate-900">{user.fullName}</div>
                      <div className="text-xs text-slate-500">{user.username} · {user.email}</div>
                    </div>
                  </div>
                </td>
                <td className="px-6 py-4">
                  <span className="inline-block px-2.5 py-1 rounded-full text-xs font-medium bg-slate-100 text-slate-600 border border-slate-200 w-36 text-center">{user.role}</span>
                </td>
                <td className="px-6 py-4">
                  <span className={`inline-block px-2.5 py-1 rounded-full text-xs font-medium w-20 text-center ${statusStyle(user.status)}`}>{user.status}</span>
                </td>
                <td className="px-6 py-4 text-sm text-slate-500">{user.lastLogin}</td>
                <td className="px-6 py-4">
                  <div className="flex gap-2">
                    <button onClick={() => openModal('edit', user)} title="Edit"
                      className="p-1.5 rounded-md text-slate-500 hover:bg-slate-100 hover:text-slate-900 transition">
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24"><path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"/><path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"/></svg>
                    </button>
                    <button onClick={() => handleDelete(user.id)} title="Delete"
                      className="p-1.5 rounded-md text-red-400 hover:bg-red-50 hover:text-red-600 transition">
                      <svg className="w-4 h-4" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24"><path d="M3 6h18"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/><line x1="10" y1="11" x2="10" y2="17"/><line x1="14" y1="11" x2="14" y2="17"/></svg>
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* Modal */}
      {showModal && (
        <div className="fixed inset-0 bg-slate-900/40 backdrop-blur-sm flex items-center justify-center z-50 p-4" onClick={closeModal}>
          <div className="bg-white rounded-2xl w-full max-w-lg shadow-2xl flex flex-col" onClick={e => e.stopPropagation()}>
            <div className="flex items-center justify-between px-6 py-4 border-b border-slate-200">
              <h2 className="text-lg font-semibold text-slate-900">{modalMode === 'add' ? 'Add New User' : 'Edit User'}</h2>
              <button onClick={closeModal} className="p-1 rounded-md text-slate-400 hover:bg-slate-100 hover:text-slate-700 transition">
                <svg className="w-5 h-5" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
              </button>
            </div>
            <div className="px-6 py-5 flex flex-col gap-4">
              {message && <p className="text-sm text-red-600">{message}</p>}
              <div className="grid grid-cols-2 gap-4">
                <div className="flex flex-col gap-1.5">
                  <label className="text-sm font-medium text-slate-700">Username *</label>
                  <input type="text" value={form.username} onChange={e => handleInputChange('username', e.target.value)} placeholder="Enter username" className={inputCls} />
                </div>
                <div className="flex flex-col gap-1.5">
                  <label className="text-sm font-medium text-slate-700">Full Name *</label>
                  <input type="text" value={form.fullName} onChange={e => handleInputChange('fullName', e.target.value)} placeholder="Enter full name" className={inputCls} />
                </div>
              </div>
              <div className="flex flex-col gap-1.5">
                <label className="text-sm font-medium text-slate-700">Email *</label>
                <input type="email" value={form.email} onChange={e => handleInputChange('email', e.target.value)} placeholder="Enter email address" className={inputCls} />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="flex flex-col gap-1.5">
                  <label className="text-sm font-medium text-slate-700">Role *</label>
                  <select value={form.role} onChange={e => handleInputChange('role', e.target.value)} className={selectCls}>
                    <option value="">Select role</option>
                    <option value="Admin">Admin</option>
                    <option value="Network Engineer">Network Engineer</option>
                    <option value="Developer">Developer</option>
                    <option value="Support">Support</option>
                    <option value="Analyst">Analyst</option>
                  </select>
                </div>
                <div className="flex flex-col gap-1.5">
                  <label className="text-sm font-medium text-slate-700">Status</label>
                  <select value={form.status} onChange={e => handleInputChange('status', e.target.value)} className={selectCls}>
                    <option value="Active">Active</option>
                    <option value="Inactive">Inactive</option>
                  </select>
                </div>
              </div>
            </div>
            <div className="flex justify-end gap-3 px-6 py-4 bg-slate-50 border-t border-slate-200 rounded-b-2xl">
              <button onClick={closeModal} className="px-4 py-2 text-sm font-medium text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition">Cancel</button>
              <button onClick={handleSubmit} disabled={!isValid}
                className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 disabled:opacity-50 disabled:cursor-not-allowed transition">
                {modalMode === 'add' ? 'Create User' : 'Update User'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
