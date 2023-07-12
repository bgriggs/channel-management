﻿namespace BigMission.ChannelManagement.Shared.Math
{
    public interface IMathRepository
    {
        public Task<IEnumerable<MathParameters>> GetParameters();
    }
}
