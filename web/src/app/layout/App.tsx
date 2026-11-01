import { Box, Container, CssBaseline } from '@mui/material'
import NavBar from './NavBar'
import ActivityDashboard from '../../features/activities/dashboard/ActivityDashboard'

function App() {
  return (
    <Box sx={{ bgcolor: '#ead5f8', minHeight: '100hv' }}>
      <CssBaseline />
      <NavBar />
      <Container maxWidth="xl" sx={{ mt: 2 }}>
        <ActivityDashboard />
      </Container>
    </Box>
  )
}

export default App
