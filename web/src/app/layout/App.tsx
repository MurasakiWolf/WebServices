import { Box, Container, CssBaseline } from '@mui/material'
import NavBar from './NavBar'
import { Outlet } from 'react-router'

function App() {
  return (
    <Box sx={{ bgcolor: '#ead5f8', minHeight: '100hv' }}>
      <CssBaseline />
      <NavBar />
      <Container maxWidth="xl" sx={{ mt: 2 }}>
        <Outlet />
      </Container>
    </Box>
  )
}

export default App
