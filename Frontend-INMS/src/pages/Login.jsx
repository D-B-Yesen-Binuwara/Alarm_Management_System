import { useMemo, useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
const serviceIdRegex = /^\d{6}$/;
const authShellStyle = {
  fontFamily: 'Manrope, Segoe UI, sans-serif',
  background:
    'radial-gradient(circle at 14% 12%, rgba(17, 181, 127, 0.18), transparent 36%), radial-gradient(circle at 86% 88%, rgba(13, 95, 135, 0.24), transparent 34%), linear-gradient(155deg, #08193a 0%, #0c2e63 44%, #0d5f87 100%)'
};
const authHeaderStyle = {
  background: 'linear-gradient(120deg, #05173a 0%, #083f6a 56%, #1b304d 100%)'
};

const Login = () => {
  const navigate = useNavigate();
  const [formData, setFormData] = useState({ identifier: '', password: '' });
  const [touched, setTouched] = useState({});
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const validate = (values) => {
    const nextErrors = {};

    const identifierValue = values.identifier.trim();

    if (!identifierValue) {
      nextErrors.identifier = 'Email or Service ID is required.';
    } else if (!emailRegex.test(identifierValue) && !serviceIdRegex.test(identifierValue)) {
      nextErrors.identifier = 'Enter a valid email or a 6-digit Service ID.';
    }

    if (!values.password) {
      nextErrors.password = 'Password is required.';
    }

    return nextErrors;
  };

  const formErrors = useMemo(() => validate(formData), [formData]);
  const isValid = Object.keys(formErrors).length === 0;

  const onChange = (event) => {
    const { name, value } = event.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    setErrors(validate({ ...formData, [name]: value }));
  };

  const onBlur = (event) => {
    const { name } = event.target;
    setTouched((prev) => ({ ...prev, [name]: true }));
    setErrors(validate(formData));
  };

  const onSubmit = async (event) => {
    event.preventDefault();
    setTouched({ identifier: true, password: true });
    const nextErrors = validate(formData);
    setErrors(nextErrors);

    if (Object.keys(nextErrors).length > 0) {
      return;
    }

    setIsSubmitting(true);
    await new Promise((resolve) => setTimeout(resolve, 700));
    localStorage.setItem('inms-authenticated', 'true');
    setIsSubmitting(false);
    navigate('/dashboard');
  };

  const showError = (name) => touched[name] && errors[name];

  return (
    <div className="min-h-screen px-4 py-8 sm:px-6 flex items-center justify-center" style={authShellStyle}>
      <div className="mx-auto w-full max-w-md overflow-hidden rounded-2xl border border-slate-300/80 bg-white shadow-2xl">
        <div className="px-6 py-5 text-white sm:px-8" style={authHeaderStyle}>
          <p className="text-xs font-semibold uppercase tracking-[0.24em] text-cyan-100/90">INMS Access</p>
          <h1 className="mt-2 text-2xl font-extrabold">Sign In</h1>
          <p className="mt-1 text-sm text-cyan-100/90">Sign in to continue to the dashboard.</p>
        </div>

        <form className="space-y-4 bg-white px-6 py-6 sm:px-8" noValidate onSubmit={onSubmit}>
          <div>
            <label htmlFor="identifier" className="mb-1.5 block text-sm font-semibold text-slate-700">Email or Service ID</label>
            <input
              id="identifier"
              name="identifier"
              type="text"
              value={formData.identifier}
              onChange={onChange}
              onBlur={onBlur}
              placeholder="name@company.com or 123456"
              className={`w-full rounded-lg border bg-white px-3 py-2.5 text-sm text-slate-800 shadow-sm outline-none transition focus:border-cyan-600 focus:ring-2 focus:ring-cyan-100 ${showError('identifier') ? 'border-rose-400 ring-2 ring-rose-100' : 'border-slate-300'}`}
            />
            {showError('identifier') && <p className="mt-1 text-xs text-rose-600">{errors.identifier}</p>}
          </div>

          <div>
            <label htmlFor="password" className="mb-1.5 block text-sm font-semibold text-slate-700">Password</label>
            <input
              id="password"
              name="password"
              type="password"
              value={formData.password}
              onChange={onChange}
              onBlur={onBlur}
              placeholder="Enter password"
              className={`w-full rounded-lg border bg-white px-3 py-2.5 text-sm text-slate-800 shadow-sm outline-none transition focus:border-cyan-600 focus:ring-2 focus:ring-cyan-100 ${showError('password') ? 'border-rose-400 ring-2 ring-rose-100' : 'border-slate-300'}`}
            />
            {showError('password') && <p className="mt-1 text-xs text-rose-600">{errors.password}</p>}
          </div>

          <button
            type="submit"
            disabled={!isValid || isSubmitting}
            className="inline-flex w-full items-center justify-center rounded-lg bg-gradient-to-r from-[#0d5f87] to-[#11b57f] px-5 py-2.5 text-sm font-bold text-white transition hover:from-[#0a4f70] hover:to-[#0da673] disabled:cursor-not-allowed disabled:opacity-50"
          >
            {isSubmitting ? 'Signing in...' : 'Sign In'}
          </button>

          <p className="text-center text-sm text-slate-600">
            New user?{' '}
            <Link to="/register" className="font-semibold text-[#0d5f87] transition hover:text-cyan-900">
              Register here
            </Link>
          </p>
        </form>
      </div>
    </div>
  );
};

export default Login;
