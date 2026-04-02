import { useParams } from 'react-router-dom';

/**
 * Placeholder page component for routes not yet implemented
 */
const PlaceholderPage = ({ title = 'Coming Soon' }) => {
  const params = useParams();
  
  // Try to get title from URL params or prop
  const pageTitle = params.title || title;

  return (
    <div className="min-h-[60vh] flex items-center justify-center p-8 bg-gray-50">
      <div className="bg-white border border-gray-200 rounded-xl shadow-sm px-8 py-10 text-center max-w-xl">
        <h1 className="text-2xl font-bold text-gray-800 mb-2">{pageTitle}</h1>
        <p className="text-sm text-gray-500">
          This screen is prepared in the frontend and can be connected as soon as the corresponding backend behavior is available.
        </p>
      </div>
    </div>
  );
};

export default PlaceholderPage;
