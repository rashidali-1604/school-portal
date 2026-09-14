import { Link, Navigate, Route, Routes } from 'react-router-dom';
import { UsersPage } from '../features/users/pages/UsersPage';
import { UserDetailsPage } from '../features/users/pages/UserDetailsPage';

export function App() {
  return (
    <div className="app-shell">
      <header className="app-shell__header">
        <Link to="/users" className="brand">School Portal</Link>
        <nav aria-label="primary">
          <Link to="/users">Users</Link>
        </nav>
      </header>
      <main id="main" className="app-shell__main">
        <Routes>
          <Route path="/" element={<Navigate to="/users" replace />} />
          <Route path="/users" element={<UsersPage />} />
          <Route path="/users/:id" element={<UserDetailsPage />} />
          <Route path="*" element={<p>Not found.</p>} />
        </Routes>
      </main>
    </div>
  );
}
