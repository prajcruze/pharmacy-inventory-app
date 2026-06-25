import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Navbar from './components/Navbar';
import MedicinesPage from './pages/MedicinesPage';
import SalesPage from './pages/SalesPage';

function App() {
  return (
    <BrowserRouter>
      <Navbar />
      <Routes>
        <Route path="/" element={<MedicinesPage />} />
        <Route path="/sales" element={<SalesPage />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;