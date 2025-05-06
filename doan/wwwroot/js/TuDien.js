// Function to change the filter and resubmit the form
function setFilter(filterValue) {
    // Create a form dynamically or use an existing one
    const form = document.createElement('form');
    form.method = 'get';
    form.action = '@Url.Action("Index", "TuDien")'; // Ensure correct URL

    const filterInput = document.createElement('input');
    filterInput.type = 'hidden';
    filterInput.name = 'filter';
    filterInput.value = filterValue;
    form.appendChild(filterInput);

    // Include current search term if it exists
    const searchInput = document.querySelector('input[name="searchTerm"]');
    if (searchInput && searchInput.value) {
        const searchInputHidden = document.createElement('input');
        searchInputHidden.type = 'hidden';
        searchInputHidden.name = 'searchTerm';
        searchInputHidden.value = searchInput.value;
        form.appendChild(searchInputHidden);
    }

    document.body.appendChild(form);
    form.submit();
}

// Add JS for audio playback later if needed
// document.querySelectorAll('.vocab-actions button').forEach(button => {
//     button.addEventListener('click', function() {
//         // Find the word/audio source and play
//         alert('Audio playback not implemented yet.');
//     });
// });