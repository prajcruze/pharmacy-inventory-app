import { Link, useLocation } from 'react-router-dom';

function Navbar() {
  const { pathname } = useLocation();

  return (
    <nav className="navbar navbar-expand navbar-dark bg-primary px-4 mb-4">
      <span className="navbar-brand fw-bold">💊 ABC Pharmacy</span>
      <div className="navbar-nav ms-auto">
        <Link
          className={`nav-link ${pathname === '/' ? 'active fw-semibold' : ''}`}
          to="/"
        >
          Medicines
        </Link>
        <Link
          className={`nav-link ${pathname === '/sales' ? 'active fw-semibold' : ''}`}
          to="/sales"
        >
          Sales
        </Link>
      </div>
    </nav>
  );
}

export default Navbar;