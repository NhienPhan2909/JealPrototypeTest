import PropTypes from 'prop-types';

export default function SyncConflictsTable({ conflicts, onResolve }) {
  if (!conflicts || conflicts.length === 0) return null;

  return (
    <div className="bg-white rounded-lg shadow p-6">
      <h2 className="text-xl font-bold text-gray-900 mb-1">Status Conflicts</h2>
      <p className="text-sm text-gray-500 mb-4">
        {conflicts.length} unresolved conflict{conflicts.length !== 1 ? 's' : ''} requiring manual review
      </p>
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-gray-200 text-sm">
          <thead className="bg-gray-50">
            <tr>
              <th className="px-4 py-2 text-left font-medium text-gray-500">Lead ID</th>
              <th className="px-4 py-2 text-left font-medium text-gray-500">EasyCars #</th>
              <th className="px-4 py-2 text-left font-medium text-gray-500">Local Status</th>
              <th className="px-4 py-2 text-left font-medium text-gray-500">Remote Status</th>
              <th className="px-4 py-2 text-left font-medium text-gray-500">Detected</th>
              <th className="px-4 py-2 text-left font-medium text-gray-500">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {conflicts.map(conflict => (
              <tr key={conflict.id} className="hover:bg-gray-50">
                <td className="px-4 py-2">{conflict.leadId}</td>
                <td className="px-4 py-2 font-mono text-xs">{conflict.easyCarsLeadNumber}</td>
                <td className="px-4 py-2">
                  <span className="px-2 py-0.5 rounded text-xs font-medium bg-blue-100 text-blue-800">
                    {conflict.localStatus}
                  </span>
                </td>
                <td className="px-4 py-2">
                  <span className="px-2 py-0.5 rounded text-xs font-medium bg-orange-100 text-orange-800">
                    {conflict.remoteStatusLabel}
                  </span>
                </td>
                <td className="px-4 py-2 text-gray-500">
                  {new Date(conflict.detectedAt).toLocaleString()}
                </td>
                <td className="px-4 py-2 flex gap-2">
                  <button
                    onClick={() => onResolve(conflict.id, 'local')}
                    className="px-3 py-1 text-xs font-medium rounded bg-blue-600 text-white hover:bg-blue-700 transition"
                  >
                    Keep Local
                  </button>
                  <button
                    onClick={() => onResolve(conflict.id, 'remote')}
                    className="px-3 py-1 text-xs font-medium rounded bg-orange-600 text-white hover:bg-orange-700 transition"
                  >
                    Use Remote
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

SyncConflictsTable.propTypes = {
  conflicts: PropTypes.arrayOf(PropTypes.shape({
    id: PropTypes.number.isRequired,
    leadId: PropTypes.number.isRequired,
    easyCarsLeadNumber: PropTypes.string.isRequired,
    localStatus: PropTypes.string.isRequired,
    remoteStatus: PropTypes.number.isRequired,
    remoteStatusLabel: PropTypes.string.isRequired,
    detectedAt: PropTypes.string.isRequired,
  })).isRequired,
  onResolve: PropTypes.func.isRequired,
};
