db.createUser({
    user: 'geofencesUser',
    pwd: 'geofencesPass',
    roles: [
        {
            role: 'readWrite',
            db: 'geofences',
        },
    ],
});
