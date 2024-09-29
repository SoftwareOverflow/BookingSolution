function getLocalDateTime() {
    let date = new Date();
    const offset = date.getTimezoneOffset();
    date = new Date(date.getTime() - (offset * 60 * 1000));

    return date.toISOString();
}

function getLocalDate() {
    return getLocalDateTime().substring(0, 10);
}
