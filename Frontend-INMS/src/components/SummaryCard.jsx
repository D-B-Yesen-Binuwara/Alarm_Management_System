// components/SummaryCard.jsx
export default function SummaryCard({ title, value, color }) {
  return (
    <div className={`border rounded-xl p-6 w-full ${color}`}>
      <h2 className="text-2xl font-bold">{value}</h2>
      <p className="text-sm text-gray-600">{title}</p>
    </div>
  );
}