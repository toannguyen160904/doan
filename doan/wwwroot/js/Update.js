fetch('/Home/UpdateVocabulary', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        Id: vocab.Id,
        TuVung: vocab.TuVung,
        PhatAm: vocab.PhatAm,
        AmHan: vocab.AmHan,
        HanTu: vocab.HanTu,
        Nghia: vocab.Nghia
    })
})
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            alert('Update successful!');
        } else {
            alert('Update failed!');
        }
    });