import './App.css'
import UserManagement from './components/UserManagement'
import Layout from './components/Layout'

function App() {
  return (
    <Layout>
      <div className="app-root">
        <UserManagement />
      </div>
    </Layout>
  )
}

export default App
