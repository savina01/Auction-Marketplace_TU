import React, { useEffect, useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import "./Navbar.css";
import logo from "../../assets/Marketplace.svg";
import NavbarProps from '../../Interfaces/ComponentProps';
import { getToken } from '../../utils/GoogleToken';
import ApiResponseDTO from '../../Interfaces/DTOs/ApiResponseDTO';
import UserService from '../../Services/UserService';
import { Dropdown } from 'flowbite-react';
import ApiService from '../../Services/ApiService';

const apiService = new ApiService;
const userService = new UserService(apiService);

const Navbar: React.FC<NavbarProps> = ({ showAuthButtons = true }) => {
  const token = getToken();
  const navigate = useNavigate();
  const [dropdownOpen, setDropdownOpen] = useState(false);
  const location = useLocation();

  const [user, setUser] = useState({
    firstName: '',
    lastName: '',
    email: '',
    profilePicture: ''
  })

  const options = [
    'one', 'two', 'three'
  ];
  const defaultOption = options[0];

  const fetchUserProfile = async () => {
    try {
      if (token) {
        const response: ApiResponseDTO = await userService.fetchUser();
        const userData = response.data;
        if (response.succeed) {
          setUser(userData);
        }
      }
    } catch (error) {
      console.error('Error during user profile fetch:', error);
    }
  };

  useEffect(() => {
    if (token) {
      fetchUserProfile();
    }
  }, [token]
  );

  const handleLogout = async () => {
    localStorage.clear();
    sessionStorage.clear();
    navigate('/login');
  };

  const isLogOutPage = location.pathname === '/login';

  return (
    <nav className="navbar">
      <div className="nav-container">
        <div className="logo-container">
          <Link to="/home">
            <img src={logo} alt="Logo" className="logo" />
          </Link>
        </div>
        <div className="nav-links-default">
          <Link to="/home" className="nav-item">
            Home
          </Link>
          <Link to="/auctions" className="nav-item">
            Auctions
          </Link>
          <Link to="/aboutUs" className="nav-item">
            About us
          </Link>
        </div>
        {showAuthButtons && (
          <>
            <div className="nav-links-user">
              <Link to="/login" className="nav-item">
                Login
              </Link>
            </div>
          </>
        )}
        {!showAuthButtons && !isLogOutPage && (
          <div className="nav-links-user">
            <div className="heart">
              <Link to="/heart" className="heart-page-link">
                {"\u2764"}
              </Link>
            </div>
            <Link to="/profile">
              <div className="profile-picture-container">
                <img
                  src={user.profilePicture}
                  alt="Loading"
                  className="profile-picture"
                />
              </div>
            </Link>
            <Dropdown
              className="dropdown-button"
              label=''
              style={{ marginLeft: '15px', marginTop: '18px' }}
              show={dropdownOpen}
              onMouseEnter={() => setDropdownOpen(true)} onMouseLeave={() => setDropdownOpen(false)}
            >
              <Dropdown.Item
                className='dropdown-item'>
                <Link to='/login' className='login-link' onClick={handleLogout}>
                  Log out
                </Link>
              </Dropdown.Item>
            </Dropdown>
          </div>
        )}
      </div>
    </nav>
  );
};

export default Navbar;