import { Box, Button, Paper, TextField, Typography } from "@mui/material";

type Props = {
    activity?: Activity
    closeForm: () => void;
    submitForm: (activity: Activity) => void;
}

export default function ActivityForm({ activity, closeForm, submitForm }: Props) {
    const handleSubmit = (event: React.FormEvent<HTMLFormElement>): void => {
        event.preventDefault();
        const formData = new FormData(event.currentTarget);
        const data: { [key: string]: FormDataEntryValue } = {};
        formData.forEach((value, key) => {
            data[key] = value;
        })

        if (activity) data.id = activity.id;

        submitForm(data as unknown as Activity);
    }
    return (
        <Paper sx={{ borderRadius: 3, padding: 3 }}>
            <Typography variant="h5" gutterBottom color="primary">
                Create activity
            </Typography>
            <Box component="form" onSubmit={handleSubmit} sx={{ display: "flex", flexDirection: "column", gap: 3 }}>
                <TextField name="title" label="Title" value={activity?.title}></TextField>
                <TextField name="description" label="Description" value={activity?.description} multiline rows={3}></TextField>
                <TextField name="category" label="Category" value={activity?.category}></TextField>
                <TextField name="date" label="" value={activity?.date} type="date"></TextField>
                <TextField name="city" label="City" value={activity?.city}></TextField>
                <TextField name="venue" label="Venue" value={activity?.venue}></TextField>
                <Box sx={{ display: "flex", justifyContent: "end", gap: 3 }}>
                    <Button onClick={closeForm} color="inherit">Cancel</Button>
                    <Button color="success" variant="contained" type="submited">Submit</Button>
                </Box>

            </Box>
        </Paper>
    )
}