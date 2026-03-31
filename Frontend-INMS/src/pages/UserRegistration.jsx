import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

const authShellStyle = {
  fontFamily: 'Manrope, Segoe UI, sans-serif',
  background:
    'radial-gradient(circle at 14% 12%, rgba(17, 181, 127, 0.18), transparent 36%), radial-gradient(circle at 86% 88%, rgba(13, 95, 135, 0.24), transparent 34%), linear-gradient(155deg, #08193a 0%, #0c2e63 44%, #0d5f87 100%)'
};
const authHeaderStyle = {
  background: 'linear-gradient(120deg, #05173a 0%, #083f6a 56%, #1b304d 100%)'
};

const roleOptions = [
  'Network Operations Engineer',
  'Regional Supervisor',
  'Province Coordinator',
  'LEA Operator'
];

const locationHierarchy = {
  Western: {
    Colombo: ['Colombo Central LEA', 'Colombo South LEA', 'Maharagama LEA'],
    Gampaha: ['Negombo LEA', 'Gampaha City LEA', 'Wattala LEA'],
    Kalutara: ['Panadura LEA', 'Kalutara Town LEA']
  },
  Central: {
    Kandy: ['Kandy City LEA', 'Peradeniya LEA'],
    Matale: ['Matale Central LEA', 'Dambulla LEA'],
    NuwaraEliya: ['Nuwara Eliya LEA', 'Hatton LEA']
  },
  Southern: {
    Galle: ['Galle Fort LEA', 'Ambalangoda LEA'],
    Matara: ['Matara Central LEA', 'Weligama LEA'],
    Hambantota: ['Hambantota LEA', 'Tangalle LEA']
  }
};

const initialFormData = {
  firstName: '',
  lastName: '',
  email: '',
  serviceId: '',
  username: '',
  password: '',
  confirmPassword: '',
  role: '',
  region: '',
  province: '',
  lea: ''
};

const getValidationErrors = (formData) => {
  const errors = {};

  if (!formData.firstName.trim()) {
    errors.firstName = 'First name is required.';
  }

  if (!formData.lastName.trim()) {
    errors.lastName = 'Last name is required.';
  }

  if (!formData.email.trim()) {
    errors.email = 'Email is required.';
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email)) {
    errors.email = 'Enter a valid email address.';
  }

  if (!formData.serviceId.trim()) {
    errors.serviceId = 'Service ID is required.';
  } else if (!/^\d{6}$/.test(formData.serviceId)) {
    errors.serviceId = 'Service ID must be exactly 6 digits.';
  }

  if (!formData.username.trim()) {
    errors.username = 'Username is required.';
  }

  if (!formData.password) {
    errors.password = 'Password is required.';
  }

  if (!formData.confirmPassword) {
    errors.confirmPassword = 'Confirm your password.';
  } else if (formData.password !== formData.confirmPassword) {
    errors.confirmPassword = 'Passwords do not match.';
  }

  if (!formData.role) {
    errors.role = 'Role is required.';
  }

  if (!formData.region) {
    errors.region = 'Region is required.';
  }

  if (!formData.province) {
    errors.province = 'Province is required.';
  }

  if (!formData.lea) {
    errors.lea = 'LEA is required.';
  }

  return errors;
};

const TextInput = ({
  id,
  name,
  label,
  value,
  onChange,
  onBlur,
  placeholder,
  type = 'text',
  error,
  disabled = false,
  maxLength,
  rightElement
}) => {
  const inputClassName = `w-full rounded-lg border bg-white px-3 py-2.5 text-sm text-slate-800 shadow-sm outline-none transition focus:border-cyan-600 focus:ring-2 focus:ring-cyan-100 ${error ? 'border-rose-400 ring-2 ring-rose-100' : 'border-slate-300'} ${disabled ? 'cursor-not-allowed bg-slate-100 text-slate-500' : ''}`;

  return (
    <div>
      <label htmlFor={id} className="mb-1.5 block text-sm font-semibold text-slate-700">
        {label}
      </label>
      <div className="relative">
        <input
          id={id}
          name={name}
          type={type}
          value={value}
          onChange={onChange}
          onBlur={onBlur}
          placeholder={placeholder}
          disabled={disabled}
          maxLength={maxLength}
          className={inputClassName}
        />
        {rightElement}
      </div>
      {error && <p className="mt-1 text-xs font-medium text-rose-600">{error}</p>}
    </div>
  );
};

const SelectInput = ({
  id,
  name,
  label,
  value,
  onChange,
  onBlur,
  options,
  placeholder,
  error,
  disabled = false
}) => {
  const selectClassName = `w-full rounded-lg border bg-white px-3 py-2.5 text-sm text-slate-800 shadow-sm outline-none transition focus:border-cyan-600 focus:ring-2 focus:ring-cyan-100 ${error ? 'border-rose-400 ring-2 ring-rose-100' : 'border-slate-300'} ${disabled ? 'cursor-not-allowed bg-slate-100 text-slate-500' : ''}`;

  return (
    <div>
      <label htmlFor={id} className="mb-1.5 block text-sm font-semibold text-slate-700">
        {label}
      </label>
      <select
        id={id}
        name={name}
        value={value}
        onChange={onChange}
        onBlur={onBlur}
        disabled={disabled}
        className={selectClassName}
      >
        <option value="">{placeholder}</option>
        {options.map((option) => (
          <option key={option.value} value={option.value}>
            {option.label}
          </option>
        ))}
      </select>
      {error && <p className="mt-1 text-xs font-medium text-rose-600">{error}</p>}
    </div>
  );
};

const UserRegistration = () => {
  const [formData, setFormData] = useState(initialFormData);
  const [errors, setErrors] = useState({});
  const [touched, setTouched] = useState({});
  const [submitAttempted, setSubmitAttempted] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [availableProvinces, setAvailableProvinces] = useState([]);
  const [availableLeas, setAvailableLeas] = useState([]);
  const [successMessage, setSuccessMessage] = useState('');

  const regionOptions = Object.keys(locationHierarchy).map((region) => ({
    value: region,
    label: region
  }));

  useEffect(() => {
    if (!formData.role) {
      setAvailableProvinces([]);
      setAvailableLeas([]);
      if (formData.region || formData.province || formData.lea) {
        setFormData((prev) => ({ ...prev, region: '', province: '', lea: '' }));
      }
      return;
    }

    if (!formData.region) {
      setAvailableProvinces([]);
      setAvailableLeas([]);
      if (formData.province || formData.lea) {
        setFormData((prev) => ({ ...prev, province: '', lea: '' }));
      }
      return;
    }

    const provinceKeys = Object.keys(locationHierarchy[formData.region] || {});
    setAvailableProvinces(provinceKeys);

    if (formData.province && !provinceKeys.includes(formData.province)) {
      setFormData((prev) => ({ ...prev, province: '', lea: '' }));
    }
  }, [formData.role, formData.region, formData.province, formData.lea]);

  useEffect(() => {
    if (!formData.role || !formData.region || !formData.province) {
      setAvailableLeas([]);
      if (formData.lea) {
        setFormData((prev) => ({ ...prev, lea: '' }));
      }
      return;
    }

    const leas = locationHierarchy[formData.region]?.[formData.province] || [];
    setAvailableLeas(leas);

    if (formData.lea && !leas.includes(formData.lea)) {
      setFormData((prev) => ({ ...prev, lea: '' }));
    }
  }, [formData.role, formData.region, formData.province, formData.lea]);

  useEffect(() => {
    if (submitAttempted || Object.keys(touched).length > 0) {
      setErrors(getValidationErrors(formData));
    }
  }, [formData, touched, submitAttempted]);

  const currentErrors = getValidationErrors(formData);
  const isFormValid = Object.keys(currentErrors).length === 0;

  const handleInputChange = (event) => {
    const { name, value } = event.target;

    setSuccessMessage('');

    if (name === 'serviceId') {
      const digitsOnly = value.replace(/\D/g, '').slice(0, 6);
      setFormData((prev) => ({ ...prev, [name]: digitsOnly }));
      return;
    }

    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleBlur = (event) => {
    const { name } = event.target;
    setTouched((prev) => ({ ...prev, [name]: true }));
  };

  const shouldShowError = (fieldName) => {
    return Boolean((touched[fieldName] || submitAttempted) && errors[fieldName]);
  };

  const resetForm = () => {
    setFormData(initialFormData);
    setTouched({});
    setErrors({});
    setSubmitAttempted(false);
    setShowPassword(false);
    setShowConfirmPassword(false);
  };

  const handleSubmit = async (event) => {
    event.preventDefault();

    setSubmitAttempted(true);
    const newErrors = getValidationErrors(formData);
    setErrors(newErrors);

    if (Object.keys(newErrors).length > 0) {
      return;
    }

    setIsSubmitting(true);
    await new Promise((resolve) => setTimeout(resolve, 1200));
    setIsSubmitting(false);

    const fullName = `${formData.firstName} ${formData.lastName}`.trim();
    setSuccessMessage(`Registration complete for ${fullName}.`);
    resetForm();
  };

  return (
    <div className="min-h-screen px-4 py-8 sm:px-6" style={authShellStyle}>
      <div className="mx-auto max-w-5xl overflow-hidden rounded-2xl border border-slate-300/80 bg-white shadow-2xl">
        <div className="px-6 py-5 text-white sm:px-8" style={authHeaderStyle}>
          <p className="text-xs font-semibold uppercase tracking-[0.24em] text-cyan-100/90">INMS Access</p>
          <h1 className="mt-2 text-2xl font-extrabold sm:text-3xl">Create New User Account</h1>
          <p className="mt-2 max-w-3xl text-sm text-cyan-100/90">
            Fill in user identity, access role, and assignment location. This form performs frontend-only validation and does not call an API.
          </p>
          <p className="mt-3 text-sm text-cyan-100/90">
            Already have access?{' '}
            <Link to="/login" className="font-semibold text-cyan-100 transition hover:text-white">
              Sign in
            </Link>
          </p>
        </div>

        <div>
          <form onSubmit={handleSubmit} noValidate className="space-y-8 bg-white px-6 py-6 sm:px-8">
            {successMessage && (
              <div className="rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm font-medium text-emerald-700">
                {successMessage}
              </div>
            )}

            <section>
              <h2 className="mb-4 text-base font-bold text-slate-800">User Details</h2>
              <div className="grid gap-4 md:grid-cols-2">
                <TextInput
                  id="firstName"
                  name="firstName"
                  label="First Name"
                  value={formData.firstName}
                  onChange={handleInputChange}
                  onBlur={handleBlur}
                  placeholder="Enter first name"
                  error={shouldShowError('firstName') ? errors.firstName : ''}
                />
                <TextInput
                  id="lastName"
                  name="lastName"
                  label="Last Name"
                  value={formData.lastName}
                  onChange={handleInputChange}
                  onBlur={handleBlur}
                  placeholder="Enter last name"
                  error={shouldShowError('lastName') ? errors.lastName : ''}
                />
                <TextInput
                  id="email"
                  name="email"
                  label="Email"
                  value={formData.email}
                  onChange={handleInputChange}
                  onBlur={handleBlur}
                  placeholder="name@company.com"
                  error={shouldShowError('email') ? errors.email : ''}
                />
              </div>

              <div className="mt-4 grid gap-4 md:grid-cols-2">
                <TextInput
                  id="serviceId"
                  name="serviceId"
                  label="Service ID"
                  value={formData.serviceId}
                  onChange={handleInputChange}
                  onBlur={handleBlur}
                  placeholder="6 digit ID"
                  maxLength={6}
                  error={shouldShowError('serviceId') ? errors.serviceId : ''}
                />
                <TextInput
                  id="username"
                  name="username"
                  label="Username"
                  value={formData.username}
                  onChange={handleInputChange}
                  onBlur={handleBlur}
                  placeholder="Enter username"
                  error={shouldShowError('username') ? errors.username : ''}
                />
              </div>

              <div className="mt-4 grid gap-4 md:grid-cols-2">
                <TextInput
                  id="password"
                  name="password"
                  label="Password"
                  type={showPassword ? 'text' : 'password'}
                  value={formData.password}
                  onChange={handleInputChange}
                  onBlur={handleBlur}
                  placeholder="Enter password"
                  error={shouldShowError('password') ? errors.password : ''}
                  rightElement={
                    <button
                      type="button"
                      onClick={() => setShowPassword((prev) => !prev)}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-xs font-semibold text-cyan-700 hover:text-cyan-900"
                    >
                      {showPassword ? 'Hide' : 'Show'}
                    </button>
                  }
                />
                <TextInput
                  id="confirmPassword"
                  name="confirmPassword"
                  label="Confirm Password"
                  type={showConfirmPassword ? 'text' : 'password'}
                  value={formData.confirmPassword}
                  onChange={handleInputChange}
                  onBlur={handleBlur}
                  placeholder="Re-enter password"
                  error={shouldShowError('confirmPassword') ? errors.confirmPassword : ''}
                  rightElement={
                    <button
                      type="button"
                      onClick={() => setShowConfirmPassword((prev) => !prev)}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-xs font-semibold text-cyan-700 hover:text-cyan-900"
                    >
                      {showConfirmPassword ? 'Hide' : 'Show'}
                    </button>
                  }
                />
              </div>
            </section>

            <section>
              <h2 className="mb-4 text-base font-bold text-slate-800">Role and Location</h2>
              <div className="grid gap-4 md:grid-cols-2">
                <SelectInput
                  id="role"
                  name="role"
                  label="Role"
                  value={formData.role}
                  onChange={handleInputChange}
                  onBlur={handleBlur}
                  placeholder="Select role"
                  options={roleOptions.map((role) => ({ value: role, label: role }))}
                  error={shouldShowError('role') ? errors.role : ''}
                />
                <SelectInput
                  id="region"
                  name="region"
                  label="Region"
                  value={formData.region}
                  onChange={handleInputChange}
                  onBlur={handleBlur}
                  placeholder={formData.role ? 'Select region' : 'Select role first'}
                  options={regionOptions}
                  disabled={!formData.role}
                  error={shouldShowError('region') ? errors.region : ''}
                />
              </div>

              <div className="mt-4 grid gap-4 md:grid-cols-2">
                <SelectInput
                  id="province"
                  name="province"
                  label="Province"
                  value={formData.province}
                  onChange={handleInputChange}
                  onBlur={handleBlur}
                  placeholder={formData.region ? 'Select province' : 'Select region first'}
                  options={availableProvinces.map((province) => ({ value: province, label: province }))}
                  disabled={!formData.role || !formData.region}
                  error={shouldShowError('province') ? errors.province : ''}
                />
                <SelectInput
                  id="lea"
                  name="lea"
                  label="LEA"
                  value={formData.lea}
                  onChange={handleInputChange}
                  onBlur={handleBlur}
                  placeholder={formData.province ? 'Select LEA' : 'Select province first'}
                  options={availableLeas.map((lea) => ({ value: lea, label: lea }))}
                  disabled={!formData.role || !formData.region || !formData.province}
                  error={shouldShowError('lea') ? errors.lea : ''}
                />
              </div>
            </section>

            <div className="flex flex-col-reverse items-stretch gap-3 border-t border-slate-200 pt-5 sm:flex-row sm:justify-end">
              <button
                type="button"
                onClick={resetForm}
                className="rounded-lg border border-slate-300 px-5 py-2.5 text-sm font-semibold text-slate-700 transition hover:bg-slate-100"
              >
                Reset
              </button>
              <button
                type="submit"
                disabled={!isFormValid || isSubmitting}
                className="inline-flex min-w-40 items-center justify-center rounded-lg bg-gradient-to-r from-[#0d5f87] to-[#11b57f] px-5 py-2.5 text-sm font-bold text-white transition hover:from-[#0a4f70] hover:to-[#0da673] disabled:cursor-not-allowed disabled:opacity-50"
              >
                {isSubmitting ? (
                  <span className="inline-flex items-center gap-2">
                    <svg className="h-4 w-4 animate-spin" viewBox="0 0 24 24" fill="none" aria-hidden="true">
                      <circle cx="12" cy="12" r="9" stroke="currentColor" strokeWidth="3" className="opacity-30" />
                      <path d="M12 3a9 9 0 0 1 9 9" stroke="currentColor" strokeWidth="3" strokeLinecap="round" />
                    </svg>
                    Submitting...
                  </span>
                ) : (
                  'Register User'
                )}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  );
};

export default UserRegistration;
