import { useMemo, useState } from 'react';
import { Link } from 'react-router-dom';
import { useUsers } from '../hooks/useUsers';
import { formatMoney } from '../../../shared/format';
import type { UserRole, UserStatus } from '../types';

const roles: UserRole[] = ['Parent', 'Student', 'Staff', 'Administrator'];
const statuses: UserStatus[] = ['Active', 'Suspended', 'Closed'];

export function UsersPage() {
  const [q, setQ] = useState('');
  const [role, setRole] = useState<UserRole | ''>('');
  const [status, setStatus] = useState<UserStatus | ''>('');
  const [page, setPage] = useState(1);

  const params = useMemo(
    () => ({ q: q || undefined, role: role || undefined, status: status || undefined, page, pageSize: 20 }),
    [q, role, status, page]
  );

  const { data, isLoading, isError, error } = useUsers(params);

  return (
    <section aria-labelledby="users-heading">
      <h1 id="users-heading">Users</h1>

      <form
        className="filters"
        role="search"
        onSubmit={(e) => {
          e.preventDefault();
          setPage(1);
        }}
      >
        <label>
          <span>Search</span>
          <input
            type="search"
            value={q}
            placeholder="Name or email"
            onChange={(e) => {
              setQ(e.target.value);
              setPage(1);
            }}
          />
        </label>
        <label>
          <span>Role</span>
          <select
            value={role}
            onChange={(e) => {
              setRole(e.target.value as UserRole | '');
              setPage(1);
            }}
          >
            <option value="">Any</option>
            {roles.map((r) => (
              <option key={r} value={r}>
                {r}
              </option>
            ))}
          </select>
        </label>
        <label>
          <span>Status</span>
          <select
            value={status}
            onChange={(e) => {
              setStatus(e.target.value as UserStatus | '');
              setPage(1);
            }}
          >
            <option value="">Any</option>
            {statuses.map((s) => (
              <option key={s} value={s}>
                {s}
              </option>
            ))}
          </select>
        </label>
      </form>

      {isLoading && <p role="status">Loading users...</p>}
      {isError && <p role="alert">Failed to load users: {(error as Error).message}</p>}

      {data && (
        <>
          <table className="users-table">
            <caption className="sr-only">List of users</caption>
            <thead>
              <tr>
                <th scope="col">Name</th>
                <th scope="col">Email</th>
                <th scope="col">Role</th>
                <th scope="col">Status</th>
                <th scope="col" className="numeric">
                  Wallet balance
                </th>
              </tr>
            </thead>
            <tbody>
              {data.items.length === 0 && (
                <tr>
                  <td colSpan={5}>No users match those filters.</td>
                </tr>
              )}
              {data.items.map((user) => (
                <tr key={user.id}>
                  <td>
                    <Link to={`/users/${user.id}`}>
                      {user.firstName} {user.lastName}
                    </Link>
                  </td>
                  <td>{user.email}</td>
                  <td>{user.role}</td>
                  <td>
                    <span className={`badge badge--${user.status.toLowerCase()}`}>{user.status}</span>
                  </td>
                  <td className="numeric">{formatMoney(user.walletBalance)}</td>
                </tr>
              ))}
            </tbody>
          </table>

          <nav className="pagination" aria-label="Pagination">
            <button type="button" disabled={data.page <= 1} onClick={() => setPage((p) => Math.max(1, p - 1))}>
              Previous
            </button>
            <span>
              Page {data.page} of {Math.max(1, data.totalPages)} ({data.totalCount} total)
            </span>
            <button
              type="button"
              disabled={data.page >= data.totalPages}
              onClick={() => setPage((p) => p + 1)}
            >
              Next
            </button>
          </nav>
        </>
      )}
    </section>
  );
}
