import { Group } from "@mui/icons-material";
import { Box, AppBar, Toolbar, Typography, Button, Container, MenuItem, MenuList } from "@mui/material";

export default function NavBar() {
    return (
        <Box sx={{ flexGrow: 1 }}>
            <AppBar position="static"
                sx={{ backgroundImage: "linear-gradient(25deg, red 0%, purple 50%, magenta 100%)" }}>
                <Container maxWidth="xl">
                    <Toolbar sx={{ display: "flex", justifyContent: "space-between" }}>
                        <MenuList>
                            <MenuItem sx={{ display: "flex", gap: 2 }}>
                                <Group fontSize="large" />
                                <Typography variant="h4" sx={{ fontWeight: "bold" }}>WebService</Typography>
                            </MenuItem>
                        </MenuList>
                        <MenuList disablePadding sx={{ display: "flex" }}>
                            <MenuItem sx={{ fontSize: "1.2rem", textTransform: "uppercase" }}>Event</MenuItem>
                            <MenuItem sx={{ fontSize: "1.2rem", textTransform: "uppercase" }}>About</MenuItem>
                            <MenuItem sx={{ fontSize: "1.2rem", textTransform: "uppercase" }}>Contact</MenuItem>
                        </MenuList>
                        <Button size="large" variant="contained" color="warning">CreateEvent</Button>
                    </Toolbar>
                </Container>
            </AppBar>
        </Box>
    )
}