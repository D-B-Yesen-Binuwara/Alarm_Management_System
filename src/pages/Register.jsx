import { useState } from "react";
import AdminRegister from "./RegisterForms/AdminRegister";
import RegionRegister from "./RegisterForms/RegionRegister";
import ProvinceRegister from "./RegisterForms/ProvinceRegister";
import LeaRegister from "./RegisterForms/LeaRegister";

function Register({ setPage }) {
  const [role, setRole] = useState("");

  if (role === "admin") return <AdminRegister setPage={setPage} />;
  if (role === "region") return <RegionRegister setPage={setPage} />;
  if (role === "province") return <ProvinceRegister setPage={setPage} />;
  if (role === "lea") return <LeaRegister setPage={setPage} />;

  return (
    <div className="auth-container">
      <h2>Select Role</h2>

      <button onClick={() => setRole("admin")}>Admin</button>
      <button onClick={() => setRole("region")}>Region Officer</button>
      <button onClick={() => setRole("province")}>Province Officer</button>
      <button onClick={() => setRole("lea")}>LEA Officer</button>

      <br /><br />

      <button onClick={() => setPage("login")}>Back</button>
    </div>
  );
}

export default Register;