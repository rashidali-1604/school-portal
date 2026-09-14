import { Link, useParams } from 'react-router-dom';
import { useUser } from '../hooks/useUsers';
import { WalletAdjustmentForm } from '../components/WalletAdjustmentForm';
import { formatDateTime, formatMoney } from '../../../shared/format';

export function UserDetailsPage() {
  const { id } = useParams<{ id: string }>();
  const { data: user, isLoading, isError, error } = useUser(id);

  if (isLoading) {
    return <p role="status">Loading user...</p>;
  }
  if (isError) {
    return <p role="alert">Failed to load user: {(error as Error).message}</p>;
  }
  if (!user) {
    return <p>Not found.</p>;
  }

  return (
    <section aria-labelledby="user-heading">
      <p>
        <Link to="/users">← Back to users</Link>
      </p>
      <h1 id="user-heading">
        {user.firstName} {user.lastName}
      </h1>

      <div className="user-summary">
        <dl>
          <div>
            <dt>Email</dt>
            <dd>{user.email}</dd>
          </div>
          <div>
            <dt>Role</dt>
            <dd>{user.role}</dd>
          </div>
          <div>
            <dt>Status</dt>
            <dd>
              <span className={`badge badge--${user.status.toLowerCase()}`}>{user.status}</span>
            </dd>
          </div>
          <div>
            <dt>Wallet balance</dt>
            <dd>{formatMoney(user.walletBalance)}</dd>
          </div>
          <div>
            <dt>Created</dt>
            <dd>{formatDateTime(user.createdAt)}</dd>
          </div>
          <div>
            <dt>Last updated</dt>
            <dd>{formatDateTime(user.updatedAt)}</dd>
          </div>
        </dl>
      </div>

      <WalletAdjustmentForm user={user} />

      <section aria-labelledby="recent-heading" className="recent">
        <h2 id="recent-heading">Recent adjustments</h2>
        {user.recentAdjustments.length === 0 ? (
          <p>No adjustments yet.</p>
        ) : (
          <table className="users-table">
            <thead>
              <tr>
                <th scope="col">When</th>
                <th scope="col">Reason</th>
                <th scope="col" className="numeric">
                  Amount
                </th>
                <th scope="col" className="numeric">
                  Balance after
                </th>
                <th scope="col">Note</th>
                <th scope="col">By</th>
              </tr>
            </thead>
            <tbody>
              {user.recentAdjustments.map((a) => (
                <tr key={a.id}>
                  <td>{formatDateTime(a.occurredAt)}</td>
                  <td>{a.reason}</td>
                  <td className="numeric">{formatMoney(a.amount)}</td>
                  <td className="numeric">{formatMoney(a.balanceAfter)}</td>
                  <td>{a.note ?? ''}</td>
                  <td>{a.performedBy ?? ''}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </section>
    </section>
  );
}
